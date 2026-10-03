using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DhcpDotNet
{
    /// <summary>
    /// A single DHCPv4 lease binding a client hardware address to an IP address for a period of time.
    /// </summary>
    public sealed class Dhcpv4Lease
    {
        public Dhcpv4Lease(byte[] clientId, IPAddress address, DateTimeOffset expiresAt)
        {
            ClientId = clientId;
            Address = address;
            ExpiresAt = expiresAt;
        }

        /// <summary>The client identifier (usually the hardware address / chaddr).</summary>
        public byte[] ClientId { get; }

        /// <summary>The leased IP address.</summary>
        public IPAddress Address { get; internal set; }

        /// <summary>The moment the lease expires.</summary>
        public DateTimeOffset ExpiresAt { get; internal set; }

        /// <summary>Whether the lease has expired.</summary>
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// A simple in-memory DHCPv4 address pool. It hands out addresses from a contiguous range, keeps a
    /// stable mapping per client, honours a requested address when it is free, and reclaims expired
    /// leases automatically. Thread-safe.
    ///
    /// Use <see cref="Offer"/> for DHCPDISCOVER (short reservation), <see cref="Acquire"/> for DHCPREQUEST
    /// (full lease), <see cref="Decline"/> and <see cref="Release"/> for DHCPDECLINE and DHCPRELEASE, and
    /// <see cref="Exclude"/> for addresses inside the range that must never be handed out.
    ///
    /// This is intended as a batteries-included starting point. For production use you will likely want
    /// to persist leases and add conflict detection (ICMP/ARP probing).
    /// </summary>
    public sealed class Dhcpv4LeasePool
    {
        private readonly object _sync = new object();
        private readonly uint _rangeStart;
        private readonly uint _rangeEnd;
        private readonly TimeSpan _leaseTime;

        // key: hex string of client id -> lease
        private readonly Dictionary<string, Dhcpv4Lease> _leasesByClient = new Dictionary<string, Dhcpv4Lease>();
        // key: address (uint) -> client key, for fast "is this address taken" checks
        private readonly Dictionary<uint, string> _clientByAddress = new Dictionary<uint, string>();
        // addresses that are never handed out
        private readonly HashSet<uint> _excluded = new HashSet<uint>();
        // declined addresses -> moment they become available again
        private readonly Dictionary<uint, DateTimeOffset> _blockedUntil = new Dictionary<uint, DateTimeOffset>();

        /// <summary>
        /// Creates a pool that allocates addresses in the inclusive range [rangeStart, rangeEnd].
        /// </summary>
        public Dhcpv4LeasePool(IPAddress rangeStart, IPAddress rangeEnd, TimeSpan leaseTime)
        {
            if (rangeStart == null) throw new ArgumentNullException(nameof(rangeStart));
            if (rangeEnd == null) throw new ArgumentNullException(nameof(rangeEnd));

            _rangeStart = ToUInt32(rangeStart);
            _rangeEnd = ToUInt32(rangeEnd);

            if (_rangeEnd < _rangeStart)
            {
                throw new ArgumentException("rangeEnd must be greater than or equal to rangeStart.");
            }

            _leaseTime = leaseTime;
        }

        /// <summary>The lease duration used when granting or renewing a lease.</summary>
        public TimeSpan LeaseTime => _leaseTime;

        /// <summary>
        /// How long <see cref="Offer"/> reserves an address for a client that has not requested it yet.
        /// Defaults to one minute.
        /// </summary>
        public TimeSpan OfferTime { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Reserves an address for a DHCPOFFER. Works like <see cref="Acquire"/>, but a new reservation only
        /// lasts <see cref="OfferTime"/>, so an offer the client never takes up does not block the address for
        /// a whole lease. An existing lease is returned unchanged and never shortened.
        /// </summary>
        /// <returns>The offered address, or <c>null</c> if the pool is exhausted.</returns>
        public IPAddress? Offer(byte[] clientId, IPAddress? requestedAddress = null)
            => Allocate(clientId, requestedAddress, OfferTime);

        /// <summary>
        /// Acquires or renews a lease for the given client. If the client already holds a lease it is
        /// renewed and its address kept. If <paramref name="requestedAddress"/> is supplied and free, it
        /// is granted. Otherwise the next free address in the range is used.
        /// </summary>
        /// <returns>The leased address, or <c>null</c> if the pool is exhausted.</returns>
        public IPAddress? Acquire(byte[] clientId, IPAddress? requestedAddress = null)
            => Allocate(clientId, requestedAddress, _leaseTime);

        /// <summary>Returns the client's active lease or offer without renewing it, or <c>null</c> if it has none.</summary>
        public Dhcpv4Lease? GetLease(byte[] clientId)
        {
            string key = ToKey(clientId);

            lock (_sync)
            {
                ReclaimExpired();
                return _leasesByClient.TryGetValue(key, out Dhcpv4Lease? lease) ? lease : null;
            }
        }

        /// <summary>
        /// Handles a DHCPDECLINE: the client found <paramref name="address"/> already in use by another device.
        /// Its lease is removed and the address is not handed out again for one <see cref="LeaseTime"/>
        /// (RFC 2131, section 4.3.3).
        /// </summary>
        /// <returns><c>true</c> if the address was leased to this client and is now blocked.</returns>
        public bool Decline(byte[] clientId, IPAddress address)
        {
            string key = ToKey(clientId);

            lock (_sync)
            {
                if (!_leasesByClient.TryGetValue(key, out Dhcpv4Lease? lease) || !lease.Address.Equals(address))
                {
                    return false;
                }

                uint value = ToUInt32(address);
                _leasesByClient.Remove(key);
                _clientByAddress.Remove(value);
                _blockedUntil[value] = DateTimeOffset.UtcNow + _leaseTime;
                return true;
            }
        }

        /// <summary>
        /// Never hands out <paramref name="address"/>, e.g. the server's own address or a device with a static
        /// address inside the range. A lease that already exists for it is kept until it ends.
        /// </summary>
        public void Exclude(IPAddress address)
        {
            uint value = ToUInt32(address);

            lock (_sync)
            {
                _excluded.Add(value);
            }
        }

        /// <summary>Releases the lease held by the given client, if any.</summary>
        public bool Release(byte[] clientId)
        {
            string key = ToKey(clientId);

            lock (_sync)
            {
                if (_leasesByClient.TryGetValue(key, out Dhcpv4Lease? lease))
                {
                    _leasesByClient.Remove(key);
                    _clientByAddress.Remove(ToUInt32(lease.Address));
                    return true;
                }
            }

            return false;
        }

        /// <summary>Returns a snapshot of the currently active (non-expired) leases.</summary>
        public IReadOnlyCollection<Dhcpv4Lease> GetActiveLeases()
        {
            lock (_sync)
            {
                ReclaimExpired();
                return _leasesByClient.Values.ToList();
            }
        }

        private IPAddress? Allocate(byte[] clientId, IPAddress? requestedAddress, TimeSpan duration)
        {
            if (clientId == null || clientId.Length == 0)
            {
                throw new ArgumentException("A non-empty client id is required.", nameof(clientId));
            }

            string key = ToKey(clientId);
            DateTimeOffset expiresAt = DateTimeOffset.UtcNow + duration;

            lock (_sync)
            {
                ReclaimExpired();

                // 1) Existing binding: keep the address and extend (never shorten) it.
                if (_leasesByClient.TryGetValue(key, out Dhcpv4Lease? existing))
                {
                    if (expiresAt > existing.ExpiresAt)
                    {
                        existing.ExpiresAt = expiresAt;
                    }

                    return existing.Address;
                }

                // 2) Honour a requested address if it is in range and free.
                if (requestedAddress != null)
                {
                    uint requested = ToUInt32(requestedAddress);
                    if (IsAvailable(requested))
                    {
                        return Bind(key, clientId, requested, expiresAt);
                    }
                }

                // 3) First free address in the range.
                for (uint candidate = _rangeStart; candidate <= _rangeEnd && candidate >= _rangeStart; candidate++)
                {
                    if (IsAvailable(candidate))
                    {
                        return Bind(key, clientId, candidate, expiresAt);
                    }
                }

                // Pool exhausted.
                return null;
            }
        }

        private bool IsAvailable(uint address)
        {
            return address >= _rangeStart && address <= _rangeEnd &&
                !_clientByAddress.ContainsKey(address) &&
                !_excluded.Contains(address) &&
                !_blockedUntil.ContainsKey(address);
        }

        private IPAddress Bind(string key, byte[] clientId, uint address, DateTimeOffset expiresAt)
        {
            IPAddress ip = ToIPAddress(address);
            Dhcpv4Lease lease = new Dhcpv4Lease(clientId, ip, expiresAt);
            _leasesByClient[key] = lease;
            _clientByAddress[address] = key;
            return ip;
        }

        private void ReclaimExpired()
        {
            List<string> expiredKeys = _leasesByClient
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (string key in expiredKeys)
            {
                Dhcpv4Lease lease = _leasesByClient[key];
                _leasesByClient.Remove(key);
                _clientByAddress.Remove(ToUInt32(lease.Address));
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;
            foreach (uint address in _blockedUntil.Where(kvp => kvp.Value <= now).Select(kvp => kvp.Key).ToList())
            {
                _blockedUntil.Remove(address);
            }
        }

        private static string ToKey(byte[] clientId) => BitConverter.ToString(clientId);

        private static uint ToUInt32(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            if (bytes.Length != 4)
            {
                throw new ArgumentException("Only IPv4 addresses are supported by Dhcpv4LeasePool.", nameof(address));
            }

            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        private static IPAddress ToIPAddress(uint value)
        {
            return new IPAddress(new byte[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value,
            });
        }
    }
}
