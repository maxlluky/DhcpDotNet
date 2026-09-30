using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DhcpDotNet
{
    /// <summary>
    /// Configuration a DHCPv4 server hands out to clients (the values that go into OFFER/ACK options).
    /// </summary>
    public sealed class Dhcpv4ServerConfiguration
    {
        /// <summary>The server's own IP address (DHCP option 54, also used as siaddr).</summary>
        public IPAddress ServerIdentifier { get; set; } = IPAddress.Any;

        /// <summary>Subnet mask handed to clients (DHCP option 1).</summary>
        public IPAddress SubnetMask { get; set; } = IPAddress.Parse("255.255.255.0");

        /// <summary>Default gateway / router (DHCP option 3). Optional.</summary>
        public IPAddress? Router { get; set; }

        /// <summary>DNS servers (DHCP option 6). Optional.</summary>
        public IList<IPAddress> DnsServers { get; } = new List<IPAddress>();

        /// <summary>Domain name (DHCP option 15). Optional.</summary>
        public string? DomainName { get; set; }

        /// <summary>Broadcast address (DHCP option 28). Optional.</summary>
        public IPAddress? BroadcastAddress { get; set; }

        /// <summary>Lease duration (DHCP option 51). Defaults to 24 hours.</summary>
        public TimeSpan LeaseTime { get; set; } = TimeSpan.FromHours(24);
    }

    /// <summary>
    /// Builds DHCPv4 server replies (OFFER, ACK, NAK) from an incoming client request. This keeps the
    /// fixed header echoing (xid, flags, giaddr, chaddr) and option formatting in one place.
    /// </summary>
    public static class Dhcpv4Reply
    {
        /// <summary>Builds a DHCPOFFER in response to a DHCPDISCOVER.</summary>
        public static Dhcpv4Packet CreateOffer(Dhcpv4Packet request, IPAddress offeredAddress, Dhcpv4ServerConfiguration config)
            => CreateReply(request, offeredAddress, Dhcpv4MessageType.Offer, config);

        /// <summary>Builds a DHCPACK in response to a DHCPREQUEST.</summary>
        public static Dhcpv4Packet CreateAck(Dhcpv4Packet request, IPAddress offeredAddress, Dhcpv4ServerConfiguration config)
            => CreateReply(request, offeredAddress, Dhcpv4MessageType.Ack, config);

        /// <summary>Builds a DHCPNAK to reject a client's request.</summary>
        public static Dhcpv4Packet CreateNak(Dhcpv4Packet request, Dhcpv4ServerConfiguration config)
        {
            Dhcpv4Packet reply = CreateHeader(request);
            reply.yiaddr = new byte[4];

            byte[] options = new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.DhcpMessageType,
                optionValue = new byte[] { (byte)Dhcpv4MessageType.Nak },
            }.buildDhcpOption();

            options = options.Concat(new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.ServerIdentifier,
                optionValue = config.ServerIdentifier.GetAddressBytes(),
            }.buildDhcpOption()).ToArray();

            reply.dhcpOptions = options;
            return reply;
        }

        private static Dhcpv4Packet CreateReply(Dhcpv4Packet request, IPAddress offeredAddress, Dhcpv4MessageType messageType, Dhcpv4ServerConfiguration config)
        {
            Dhcpv4Packet reply = CreateHeader(request);
            reply.yiaddr = offeredAddress.GetAddressBytes();
            reply.siaddr = config.ServerIdentifier.GetAddressBytes();

            List<byte> options = new List<byte>();

            options.AddRange(new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.DhcpMessageType,
                optionValue = new byte[] { (byte)messageType },
            }.buildDhcpOption());

            options.AddRange(new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.ServerIdentifier,
                optionValue = config.ServerIdentifier.GetAddressBytes(),
            }.buildDhcpOption());

            options.AddRange(new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.IpAddressLeaseTime,
                optionValue = GetBigEndianUInt32((uint)config.LeaseTime.TotalSeconds),
            }.buildDhcpOption());

            options.AddRange(new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.Subnetmask,
                optionValue = config.SubnetMask.GetAddressBytes(),
            }.buildDhcpOption());

            if (config.Router != null)
            {
                options.AddRange(new Dhcpv4Option
                {
                    optionId = Dhcpv4OptionIds.Router,
                    optionValue = config.Router.GetAddressBytes(),
                }.buildDhcpOption());
            }

            if (config.DnsServers.Count > 0)
            {
                byte[] dnsBytes = config.DnsServers.SelectMany(ip => ip.GetAddressBytes()).ToArray();
                options.AddRange(new Dhcpv4Option
                {
                    optionId = Dhcpv4OptionIds.DomainNameServer,
                    optionValue = dnsBytes,
                }.buildDhcpOption());
            }

            if (!string.IsNullOrEmpty(config.DomainName))
            {
                options.AddRange(new Dhcpv4Option
                {
                    optionId = Dhcpv4OptionIds.DomainName,
                    optionValue = System.Text.Encoding.ASCII.GetBytes(config.DomainName!),
                }.buildDhcpOption());
            }

            if (config.BroadcastAddress != null)
            {
                options.AddRange(new Dhcpv4Option
                {
                    optionId = Dhcpv4OptionIds.BroadcastAddress,
                    optionValue = config.BroadcastAddress.GetAddressBytes(),
                }.buildDhcpOption());
            }

            reply.dhcpOptions = options.ToArray();
            return reply;
        }

        private static Dhcpv4Packet CreateHeader(Dhcpv4Packet request)
        {
            return new Dhcpv4Packet
            {
                op = 0x02, // BOOTREPLY
                htype = request.htype,
                hlen = request.hlen,
                hops = 0x00,
                xid = request.xid,
                secs = new byte[2],
                flags = request.flags,
                ciaddr = new byte[4],
                giaddr = request.giaddr,
                chaddr = request.chaddr,
                chaddrPadding = request.chaddrPadding,
            };
        }

        private static byte[] GetBigEndianUInt32(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }
    }
}
