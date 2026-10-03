using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DhcpDotNet
{
    /// <summary>
    /// A lightweight asynchronous DHCPv4 server host. It binds a UDP socket to the DHCP server port
    /// (67), receives datagrams, parses them into <see cref="Dhcpv4Packet"/> instances and raises
    /// <see cref="PacketReceived"/> for each one. Your handler decides how to respond and returns a
    /// reply packet (or <c>null</c> to stay silent).
    ///
    /// The host is transport only; it contains no lease logic of its own. Combine it with
    /// <see cref="Dhcpv4LeasePool"/> and <see cref="Dhcpv4Reply"/> to build a working server.
    ///
    /// Cross-platform note: binding to UDP port 67 is privileged. On Linux run the process as root or
    /// grant the binary the capability once with:
    /// <c>sudo setcap 'cap_net_bind_service=+ep' /path/to/your/app</c>.
    ///
    /// On a machine with more than one network, create the server with <see cref="ForInterface(string, int)"/>
    /// so it only answers clients on that interface and its broadcasts leave through it.
    /// </summary>
    public sealed class Dhcpv4Server : IDisposable
    {
        /// <summary>Standard DHCP server port.</summary>
        public const int ServerPort = 67;

        /// <summary>Standard DHCP client port.</summary>
        public const int ClientPort = 68;

        // RFC 1542: some BOOTP/DHCP clients drop replies shorter than the original 300 byte BOOTP message.
        private const int MinimumReplySize = 300;

        // Windows: stop ICMP "port unreachable" answers to our unicasts from failing the next receive.
        private const int SioUdpConnReset = unchecked((int)0x9800000C);

        private readonly IPAddress _bindAddress;
        private readonly int _listenPort;
        private readonly NetworkInterface? _networkInterface;
        private UdpClient? _udpClient;
        private CancellationTokenSource? _cts;
        private Task? _receiveLoop;

        /// <summary>
        /// Creates a new DHCPv4 server host.
        /// </summary>
        /// <param name="bindAddress">Local address to bind to. Defaults to <see cref="IPAddress.Any"/> (all interfaces).</param>
        /// <param name="listenPort">UDP port to listen on. Defaults to 67.</param>
        public Dhcpv4Server(IPAddress? bindAddress = null, int listenPort = ServerPort)
        {
            _bindAddress = bindAddress ?? IPAddress.Any;
            _listenPort = listenPort;
        }

        private Dhcpv4Server(NetworkInterface networkInterface, int listenPort)
        {
            _networkInterface = networkInterface ?? throw new ArgumentNullException(nameof(networkInterface));
            _bindAddress = IPAddress.Any;
            _listenPort = listenPort;
        }

        /// <summary>
        /// Creates a server that only receives from and sends through one network interface. Without this a
        /// server bound to all interfaces answers clients on every network the machine is connected to.
        /// Uses SO_BINDTODEVICE on Linux, IP_BOUND_IF on macOS and the interface's IPv4 address on Windows.
        /// Linux and macOS need the net8.0 build of this library.
        /// </summary>
        /// <param name="interfaceName">Interface name or id, e.g. <c>eth0</c> or <c>Ethernet</c>.</param>
        /// <param name="listenPort">UDP port to listen on. Defaults to 67.</param>
        public static Dhcpv4Server ForInterface(string interfaceName, int listenPort = ServerPort)
        {
            NetworkInterface? networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(nic =>
                string.Equals(nic.Name, interfaceName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(nic.Id, interfaceName, StringComparison.OrdinalIgnoreCase));

            return ForInterface(networkInterface ?? throw new ArgumentException($"Network interface '{interfaceName}' not found.", nameof(interfaceName)), listenPort);
        }

        /// <inheritdoc cref="ForInterface(string, int)"/>
        /// <param name="networkInterface">The interface to serve.</param>
        /// <param name="listenPort">UDP port to listen on. Defaults to 67.</param>
        public static Dhcpv4Server ForInterface(NetworkInterface networkInterface, int listenPort = ServerPort)
            => new Dhcpv4Server(networkInterface, listenPort);

        /// <summary>Raised for every successfully parsed DHCPv4 packet. Return a reply to send it back.</summary>
        public event EventHandler<Dhcpv4PacketReceivedEventArgs>? PacketReceived;

        /// <summary>Raised when the receive loop encounters a non-fatal error.</summary>
        public event EventHandler<Exception>? Error;

        /// <summary>Whether the server is currently listening.</summary>
        public bool IsRunning => _receiveLoop != null && !_receiveLoop.IsCompleted;

        /// <summary>
        /// Starts listening in the background and returns immediately. Call <see cref="StopAsync"/> to stop.
        /// </summary>
        public void Start() => StartCore(CancellationToken.None);

        /// <summary>
        /// Runs the receive loop until the provided token is cancelled, then closes the socket and returns
        /// normally. Useful for hosting the server directly on a long-lived task (for example from <c>Main</c>).
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Task receiveLoop = StartCore(cancellationToken);

            try
            {
                await receiveLoop.ConfigureAwait(false);
            }
            finally
            {
                await StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>Stops the receive loop and releases the socket.</summary>
        public async Task StopAsync()
        {
            // Take ownership first so concurrent or repeated calls do nothing twice.
            CancellationTokenSource? cts = Interlocked.Exchange(ref _cts, null);
            UdpClient? udpClient = Interlocked.Exchange(ref _udpClient, null);
            Task? receiveLoop = Interlocked.Exchange(ref _receiveLoop, null);

            cts?.Cancel();
            udpClient?.Dispose(); // aborts a pending receive

            if (receiveLoop != null)
            {
                try
                {
                    await receiveLoop.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // expected on shutdown
                }
            }

            cts?.Dispose();
        }

        /// <summary>
        /// Sends a raw DHCPv4 payload to the given endpoint. Payloads shorter than the 300 byte BOOTP
        /// minimum are padded with zeros.
        /// </summary>
        public Task SendAsync(byte[] payload, IPEndPoint destination)
        {
            UdpClient udpClient = _udpClient ?? throw new InvalidOperationException("The server socket is not open. Call Start or RunAsync first.");
            return SendAsync(udpClient, payload, destination);
        }

        /// <summary>
        /// Broadcasts a DHCPv4 reply packet to the client port (255.255.255.255:68). This is the safe
        /// default because a client acquiring a lease usually cannot yet be reached by unicast.
        /// </summary>
        public Task BroadcastReplyAsync(Dhcpv4Packet reply)
        {
            return SendAsync(reply.buildPacket(), new IPEndPoint(IPAddress.Broadcast, ClientPort));
        }

        private static async Task SendAsync(UdpClient udpClient, byte[] payload, IPEndPoint destination)
        {
            if (payload.Length < MinimumReplySize)
            {
                // Zero bytes after the End option are padding.
                Array.Resize(ref payload, MinimumReplySize);
            }

            await udpClient.SendAsync(payload, payload.Length, destination).ConfigureAwait(false);
        }

        private Task StartCore(CancellationToken cancellationToken)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _udpClient = CreateSocket();
            _receiveLoop = ReceiveLoopAsync(_udpClient, _cts.Token);
            return _receiveLoop;
        }

        private UdpClient CreateSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.EnableBroadcast = true;

                IPAddress bindAddress = _bindAddress;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    socket.IOControl(SioUdpConnReset, new byte[4], null);

                    if (_networkInterface != null)
                    {
                        // Windows delivers broadcasts to a socket bound to the interface address and sends from that interface.
                        bindAddress = GetIPv4Address(_networkInterface);
                    }
                }
                else if (_networkInterface != null)
                {
                    // Unix only delivers broadcasts to sockets bound to the wildcard address, so pin the socket to the device instead.
                    BindToDevice(socket, _networkInterface);
                }

                socket.Bind(new IPEndPoint(bindAddress, _listenPort));
                return new UdpClient { Client = socket, EnableBroadcast = true };
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        private static void BindToDevice(Socket socket, NetworkInterface networkInterface)
        {
#if NET
            if (OperatingSystem.IsLinux())
            {
                socket.SetRawSocketOption(1 /* SOL_SOCKET */, 25 /* SO_BINDTODEVICE */, Encoding.ASCII.GetBytes(networkInterface.Name + "\0"));
                return;
            }

            if (OperatingSystem.IsMacOS())
            {
                int index = networkInterface.GetIPProperties().GetIPv4Properties().Index;
                socket.SetRawSocketOption(0 /* IPPROTO_IP */, 25 /* IP_BOUND_IF */, BitConverter.GetBytes(index));
                return;
            }
#endif
            throw new PlatformNotSupportedException("Binding to a network interface is supported on Windows, and on Linux and macOS with the net8.0 build.");
        }

        private static IPAddress GetIPv4Address(NetworkInterface networkInterface)
        {
            return networkInterface.GetIPProperties().UnicastAddresses
                .Select(unicast => unicast.Address)
                .FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork)
                ?? throw new InvalidOperationException($"Network interface '{networkInterface.Name}' has no IPv4 address.");
        }

        private async Task ReceiveLoopAsync(UdpClient udpClient, CancellationToken cancellationToken)
        {
            // A pending receive cannot be cancelled on every target framework; closing the socket aborts it.
            using CancellationTokenRegistration registration = cancellationToken.Register(udpClient.Dispose);

            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result;
                try
                {
                    result = await udpClient.ReceiveAsync().ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    break; // socket closed during shutdown
                }
                catch (Exception) when (cancellationToken.IsCancellationRequested)
                {
                    break; // the socket was closed to stop the server
                }
                catch (Exception ex)
                {
                    Error?.Invoke(this, ex);
                    continue;
                }

                Dhcpv4Packet packet = new Dhcpv4Packet();
                if (!packet.parsePacket(result.Buffer))
                {
                    continue; // not a valid DHCP payload
                }

                Dhcpv4PacketReceivedEventArgs args = new Dhcpv4PacketReceivedEventArgs(packet, result.RemoteEndPoint);

                try
                {
                    PacketReceived?.Invoke(this, args);
                }
                catch (Exception ex)
                {
                    Error?.Invoke(this, ex);
                    continue;
                }

                if (args.Reply != null)
                {
                    try
                    {
                        IPEndPoint destination = args.ReplyEndPoint ?? new IPEndPoint(IPAddress.Broadcast, ClientPort);
                        await SendAsync(udpClient, args.Reply.buildPacket(), destination).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Error?.Invoke(this, ex);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            CancellationTokenSource? cts = Interlocked.Exchange(ref _cts, null);
            UdpClient? udpClient = Interlocked.Exchange(ref _udpClient, null);
            _receiveLoop = null;

            try
            {
                cts?.Cancel();
                udpClient?.Dispose();
                cts?.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    /// Carries a received DHCPv4 packet to a <see cref="Dhcpv4Server.PacketReceived"/> handler and lets
    /// the handler set a reply to be sent automatically.
    /// </summary>
    public sealed class Dhcpv4PacketReceivedEventArgs : EventArgs
    {
        public Dhcpv4PacketReceivedEventArgs(Dhcpv4Packet packet, IPEndPoint remoteEndPoint)
        {
            Packet = packet;
            RemoteEndPoint = remoteEndPoint;
        }

        /// <summary>The parsed DHCPv4 packet.</summary>
        public Dhcpv4Packet Packet { get; }

        /// <summary>The endpoint the packet was received from.</summary>
        public IPEndPoint RemoteEndPoint { get; }

        /// <summary>The DHCP message type of the request, if present.</summary>
        public Dhcpv4MessageType? MessageType => Packet.getMessageType();

        /// <summary>
        /// Set this to have the server send a reply. Leave it <c>null</c> to stay silent.
        /// </summary>
        public Dhcpv4Packet? Reply { get; set; }

        /// <summary>
        /// Optional destination for <see cref="Reply"/>. Defaults to a broadcast to 255.255.255.255:68.
        /// </summary>
        public IPEndPoint? ReplyEndPoint { get; set; }
    }
}
