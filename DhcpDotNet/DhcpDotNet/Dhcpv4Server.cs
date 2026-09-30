using System;
using System.Net;
using System.Net.Sockets;
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
    /// </summary>
    public sealed class Dhcpv4Server : IDisposable
    {
        /// <summary>Standard DHCP server port.</summary>
        public const int ServerPort = 67;

        /// <summary>Standard DHCP client port.</summary>
        public const int ClientPort = 68;

        private readonly IPAddress _bindAddress;
        private readonly int _listenPort;
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

        /// <summary>Raised for every successfully parsed DHCPv4 packet. Return a reply to send it back.</summary>
        public event EventHandler<Dhcpv4PacketReceivedEventArgs>? PacketReceived;

        /// <summary>Raised when the receive loop encounters a non-fatal error.</summary>
        public event EventHandler<Exception>? Error;

        /// <summary>Whether the server is currently listening.</summary>
        public bool IsRunning => _receiveLoop != null && !_receiveLoop.IsCompleted;

        /// <summary>
        /// Starts listening in the background and returns immediately. Call <see cref="StopAsync"/> to stop.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            _cts = new CancellationTokenSource();
            _udpClient = CreateSocket();
            _receiveLoop = ReceiveLoopAsync(_udpClient, _cts.Token);
        }

        /// <summary>
        /// Runs the receive loop until the provided token is cancelled. Useful for hosting the server
        /// directly on a long-lived task (for example from <c>Main</c>).
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The server is already running.");
            }

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _udpClient = CreateSocket();
            _receiveLoop = ReceiveLoopAsync(_udpClient, _cts.Token);
            await _receiveLoop.ConfigureAwait(false);
        }

        /// <summary>Stops the receive loop and releases the socket.</summary>
        public async Task StopAsync()
        {
            _cts?.Cancel();

            try
            {
                _udpClient?.Close();
            }
            catch
            {
                // ignored; closing unblocks the pending ReceiveAsync
            }

            if (_receiveLoop != null)
            {
                try
                {
                    await _receiveLoop.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // expected on shutdown
                }
            }

            _receiveLoop = null;
            _cts?.Dispose();
            _cts = null;
            _udpClient = null;
        }

        /// <summary>
        /// Sends a raw DHCPv4 payload to the given endpoint.
        /// </summary>
        public async Task SendAsync(byte[] payload, IPEndPoint destination)
        {
            if (_udpClient == null)
            {
                throw new InvalidOperationException("The server socket is not open. Call Start or RunAsync first.");
            }

            await _udpClient.SendAsync(payload, payload.Length, destination).ConfigureAwait(false);
        }

        /// <summary>
        /// Broadcasts a DHCPv4 reply packet to the client port (255.255.255.255:68). This is the safe
        /// default because a client acquiring a lease usually cannot yet be reached by unicast.
        /// </summary>
        public Task BroadcastReplyAsync(Dhcpv4Packet reply)
        {
            return SendAsync(reply.buildPacket(), new IPEndPoint(IPAddress.Broadcast, ClientPort));
        }

        private UdpClient CreateSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.EnableBroadcast = true;
            socket.Bind(new IPEndPoint(_bindAddress, _listenPort));

            return new UdpClient { Client = socket, EnableBroadcast = true };
        }

        private async Task ReceiveLoopAsync(UdpClient udpClient, CancellationToken cancellationToken)
        {
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
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
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
                        await SendAsync(args.Reply.buildPacket(), destination).ConfigureAwait(false);
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
            try
            {
                _cts?.Cancel();
                _udpClient?.Dispose();
                _cts?.Dispose();
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
