<a>
    <img src="DhcpDotNet/logo.png" alt="DhcpDotNet" align="right" height="60" />
</a>

# DhcpDotNet

Managed, cross-platform (Windows, macOS, Linux) .NET library to **build and parse DHCPv4 and DHCPv6
packets** — plus a small async **DHCPv4 server** to run a working DHCP server in a few lines.

Follows RFC 2131/2132 (DHCPv4), RFC 8415 (DHCPv6) and the
[IANA assignments](https://www.iana.org/assignments/bootp-dhcp-parameters/bootp-dhcp-parameters.xhtml).

## Requirements

Targets **.NET Standard 2.0** and **.NET 8.0** (runs on .NET 8+, .NET Framework 4.6.1+, Mono/Xamarin).
Building from source needs the [.NET SDK 8.0](https://dotnet.microsoft.com/download) or newer.

```bash
cd DhcpDotNet        # folder containing DhcpDotNet.sln
dotnet build
dotnet test
```

## Build and parse a packet

```csharp
using System.Net.NetworkInformation;
using DhcpDotNet;

var messageType = new Dhcpv4Option
{
    optionId = Dhcpv4OptionIds.DhcpMessageType,
    optionValue = new byte[] { (byte)Dhcpv4MessageType.Discover },
};

var discover = new Dhcpv4Packet
{
    op = 0x01,                                   // BOOTREQUEST
    xid = new byte[] { 0x00, 0x01, 0x02, 0x03 },
    chaddr = PhysicalAddress.Parse("00-11-22-33-44-55").GetAddressBytes(),
    dhcpOptions = messageType.buildDhcpOption(),
};
byte[] payload = discover.buildPacket();         // ready to send over UDP

var packet = new Dhcpv4Packet();
if (packet.parsePacket(payload))
{
    Dhcpv4MessageType? type = packet.getMessageType();
    foreach (Dhcpv4Option option in packet.getOptions()) { /* ... */ }
}
```

DHCPv6 works the same way with `Dhcpv6Packet`, `Dhcpv6Option`, `Dhcpv6MessageType` and `Dhcpv6OptionIds`.

## Run a DHCPv4 server

```csharp
using System.Net;
using DhcpDotNet;

var config = new Dhcpv4ServerConfiguration
{
    ServerIdentifier = IPAddress.Parse("192.168.50.1"),
    SubnetMask = IPAddress.Parse("255.255.255.0"),
    Router = IPAddress.Parse("192.168.50.1"),
    LeaseTime = TimeSpan.FromHours(12),
};
config.DnsServers.Add(IPAddress.Parse("1.1.1.1"));

var pool = new Dhcpv4LeasePool(
    IPAddress.Parse("192.168.50.100"), IPAddress.Parse("192.168.50.200"), config.LeaseTime);
pool.Exclude(config.ServerIdentifier);           // never hand out the server's own address

using var server = Dhcpv4Server.ForInterface("eth0");   // or new Dhcpv4Server() for all interfaces
server.PacketReceived += (_, e) =>
{
    switch (e.MessageType)
    {
        case Dhcpv4MessageType.Discover:
            var offer = pool.Offer(e.Packet.chaddr);
            if (offer != null) e.Reply = Dhcpv4Reply.CreateOffer(e.Packet, offer, config);
            break;
        case Dhcpv4MessageType.Request:
            var lease = pool.Acquire(e.Packet.chaddr);
            e.Reply = lease != null
                ? Dhcpv4Reply.CreateAck(e.Packet, lease, config)
                : Dhcpv4Reply.CreateNak(e.Packet, config);
            break;
        case Dhcpv4MessageType.Release:
            pool.Release(e.Packet.chaddr);
            break;
    }
};

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
await server.RunAsync(cts.Token);                // returns on Ctrl+C
```

`Dhcpv4LeasePool` also provides `Decline`, `GetLease` and `OfferTime`.
A complete version is in [`samples/DhcpServerSample`](DhcpDotNet/samples/DhcpServerSample).

### Linux

UDP port 67 is privileged. Run as root, or grant the binary the bind capability once:

```bash
cd DhcpDotNet/samples/DhcpServerSample
dotnet build -c Release
sudo setcap 'cap_net_bind_service=+ep' bin/Release/net8.0/DhcpServerSample
./bin/Release/net8.0/DhcpServerSample 192.168.50.1 eth0
```

> ⚠️ Don't run a second DHCP server on a network that already has one — it disrupts existing clients.

## Changelog

**3.0.0**
- SDK-style project targeting .NET Standard 2.0 + .NET 8.0; nullable reference types, XML docs, xUnit tests.
- New: `Dhcpv4Server`, `Dhcpv4LeasePool`, `Dhcpv4Reply`, `Dhcpv4ServerConfiguration`, message-type enums,
  `getOptions()` / `getMessageType()`.
- Fixed: stray padding in `buildPacket()`, DHCPv6 classes now public with correct 2-octet encoding,
  more robust option parsing, unknown option codes round-trip.
- Removed the legacy Windows-only example projects and vendored `packages/`.

2.x releases remain available on [NuGet](https://www.nuget.org/packages/DhcpDotNet/).

## Contributing and license

See [CONTRIBUTING.md](CONTRIBUTING.md). Released under the [MIT License](LICENSE).
Created and maintained by [MaxlLuky](https://github.com/maxlluky).
