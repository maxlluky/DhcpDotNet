using System.Net;
using System.Net.Sockets;
using DhcpDotNet;

// -----------------------------------------------------------------------------
// Minimal working DHCPv4 server built on DhcpDotNet.
//
// It listens on UDP :67, offers addresses from a pool and answers DISCOVER with
// OFFER and REQUEST with ACK. Run it on Linux with:
//
//     sudo dotnet run
//
// or, to run the built binary without root every time, grant the bind capability
// once:
//
//     sudo setcap 'cap_net_bind_service=+ep' ./bin/Debug/net8.0/DhcpServerSample
//
// Point a client on the same layer-2 segment at this host to see it hand out a
// lease. WARNING: do not run a second DHCP server on a network that already has
// one unless you know what you are doing.
// -----------------------------------------------------------------------------

// The address of the interface this server runs on. Clients receive this as the
// router/gateway and DHCP server identifier by default. Adjust to your network.
IPAddress serverAddress = IPAddress.Parse(args.Length > 0 ? args[0] : "192.168.50.1");

var config = new Dhcpv4ServerConfiguration
{
    ServerIdentifier = serverAddress,
    SubnetMask = IPAddress.Parse("255.255.255.0"),
    Router = serverAddress,
    DomainName = "lan",
    LeaseTime = TimeSpan.FromHours(12),
};
config.DnsServers.Add(IPAddress.Parse("1.1.1.1"));
config.DnsServers.Add(IPAddress.Parse("8.8.8.8"));

// Address pool to hand out (inclusive range).
var pool = new Dhcpv4LeasePool(
    rangeStart: IPAddress.Parse("192.168.50.100"),
    rangeEnd: IPAddress.Parse("192.168.50.200"),
    leaseTime: config.LeaseTime);

using var server = new Dhcpv4Server();

server.Error += (_, ex) => Console.WriteLine($"[error] {ex.Message}");

server.PacketReceived += (_, e) =>
{
    byte[] clientId = e.Packet.chaddr;
    string mac = BitConverter.ToString(clientId);

    switch (e.MessageType)
    {
        case Dhcpv4MessageType.Discover:
        {
            IPAddress? offer = pool.Acquire(clientId, GetRequestedAddress(e.Packet));
            if (offer == null)
            {
                Console.WriteLine($"[discover] {mac} -> pool exhausted, ignoring");
                return;
            }

            Console.WriteLine($"[discover] {mac} -> offering {offer}");
            e.Reply = Dhcpv4Reply.CreateOffer(e.Packet, offer, config);
            break;
        }

        case Dhcpv4MessageType.Request:
        {
            IPAddress? lease = pool.Acquire(clientId, GetRequestedAddress(e.Packet));
            if (lease == null)
            {
                Console.WriteLine($"[request]  {mac} -> no address available, sending NAK");
                e.Reply = Dhcpv4Reply.CreateNak(e.Packet, config);
                return;
            }

            Console.WriteLine($"[request]  {mac} -> ACK {lease} (lease {config.LeaseTime.TotalHours:0}h)");
            e.Reply = Dhcpv4Reply.CreateAck(e.Packet, lease, config);
            break;
        }

        case Dhcpv4MessageType.Release:
            pool.Release(clientId);
            Console.WriteLine($"[release]  {mac} -> lease released");
            break;

        default:
            Console.WriteLine($"[{e.MessageType}] from {mac}");
            break;
    }
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

Console.WriteLine($"DHCPv4 server listening on :67 (server id {serverAddress}). Press Ctrl+C to stop.");

try
{
    await server.RunAsync(cts.Token);
}
catch (OperationCanceledException)
{
    // graceful shutdown
}
catch (SocketException ex)
{
    Console.WriteLine($"Could not bind to UDP :67 ({ex.Message}).");
    Console.WriteLine("On Linux run with sudo, or grant cap_net_bind_service to the binary.");
    return 1;
}

Console.WriteLine("Server stopped.");
return 0;

// Reads the client's requested IP address (DHCP option 50), if present.
static IPAddress? GetRequestedAddress(Dhcpv4Packet packet)
{
    foreach (Dhcpv4Option option in packet.getOptions())
    {
        if (option.optionIdBytes == (byte)Dhcpv4OptionIds.RequestedIpAddress && option.optionValue.Length == 4)
        {
            return new IPAddress(option.optionValue);
        }
    }

    return null;
}
