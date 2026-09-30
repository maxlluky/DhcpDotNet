<a>
    <img src="DhcpDotNet/logo.png" alt="DhcpDotNet" align="right" height="60" />
</a>

# DhcpDotNet

Fully managed, cross-platform (Windows, macOS, **Linux**) .NET library for building, parsing,
sending and receiving **DHCPv4** and **DHCPv6** packets — plus a high-level asynchronous DHCPv4
server host so you can stand up a working DHCP server in a few lines of code.

DhcpDotNet follows the [IANA](https://www.iana.org/assignments/bootp-dhcp-parameters/bootp-dhcp-parameters.xhtml)
assignments and RFC 2131 / 2132 (DHCPv4) and RFC 8415 (DHCPv6).

- **Build** DHCPv4 / DHCPv6 packets with full control over every field and option
- **Parse** received packets and read every option
- **Send / receive** via the built-in `UdpClient`/`Socket` transport
- **Serve** leases with the batteries-included `Dhcpv4Server`, `Dhcpv4LeasePool` and `Dhcpv4Reply` helpers

## Requirements

- The library targets **.NET Standard 2.0** and **.NET 8.0**, so it runs on .NET 8+, .NET Framework 4.6.1+,
  and Mono/Xamarin.
- To build from source you need the [.NET SDK 8.0](https://dotnet.microsoft.com/download) or newer.

## Project layout

```
DhcpDotNet/
├─ DhcpDotNet/               # the library (Dhcpv4/Dhcpv6 packets, server, lease pool)
├─ samples/DhcpServerSample/ # a runnable DHCPv4 server you can start on Linux
└─ tests/DhcpDotNet.Tests/   # xUnit test suite
```

## Build & test

```bash
cd DhcpDotNet          # the folder containing DhcpDotNet.sln
dotnet build
dotnet test
```

## Quick start — build a DHCPv4 packet

```csharp
using System.Net;
using System.Net.NetworkInformation;
using DhcpDotNet;

// A DHCP option (here: message type = DISCOVER).
Dhcpv4Option messageType = new Dhcpv4Option
{
    optionId = Dhcpv4OptionIds.DhcpMessageType,
    optionValue = new byte[] { (byte)Dhcpv4MessageType.Discover },
};
// optionLength is derived from the value automatically.

Dhcpv4Packet discover = new Dhcpv4Packet
{
    op = 0x01,                                   // BOOTREQUEST
    xid = new byte[] { 0x00, 0x01, 0x02, 0x03 },
    chaddr = PhysicalAddress.Parse("00-11-22-33-44-55").GetAddressBytes(),
    dhcpOptions = messageType.buildDhcpOption(),
};

byte[] payload = discover.buildPacket();          // ready to send over UDP
```

Parsing works the same way in reverse:

```csharp
Dhcpv4Packet packet = new Dhcpv4Packet();
if (packet.parsePacket(payload))
{
    Dhcpv4MessageType? type = packet.getMessageType();
    foreach (Dhcpv4Option option in packet.getOptions())
    {
        // read each option
    }
}
```

## Quick start — run a DHCPv4 server

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
    IPAddress.Parse("192.168.50.100"),
    IPAddress.Parse("192.168.50.200"),
    config.LeaseTime);

using var server = new Dhcpv4Server();
server.PacketReceived += (_, e) =>
{
    switch (e.MessageType)
    {
        case Dhcpv4MessageType.Discover:
            var offer = pool.Acquire(e.Packet.chaddr);
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

await server.RunAsync(CancellationToken.None);
```

A complete, runnable version lives in [`samples/DhcpServerSample`](DhcpDotNet/samples/DhcpServerSample).

### Running on Linux

Binding to UDP port 67 is privileged. Either run as root:

```bash
cd DhcpDotNet/samples/DhcpServerSample
sudo dotnet run -- 192.168.50.1
```

…or grant the built binary the bind capability once so it can run unprivileged:

```bash
dotnet build -c Release
sudo setcap 'cap_net_bind_service=+ep' bin/Release/net8.0/DhcpServerSample
./bin/Release/net8.0/DhcpServerSample 192.168.50.1
```

> ⚠️ Do **not** run a second DHCP server on a network that already has one unless you know exactly
> what you are doing — it will interfere with existing clients.

## DHCPv6

`Dhcpv6Packet`, `Dhcpv6Option`, `Dhcpv6MessageType` and `Dhcpv6OptionIds` mirror the DHCPv4 API for
building and parsing DHCPv6 messages (RFC 8415). Option codes and lengths are encoded as 2-octet
big-endian values per the spec.

## Changelog

### 3.0.0
- Modernised the project to SDK-style multi-targeting (**.NET Standard 2.0 + .NET 8.0**), C# `latest`,
  nullable reference types and XML docs.
- **New:** high-level `Dhcpv4Server` async host, `Dhcpv4LeasePool` address pool, `Dhcpv4Reply`
  OFFER/ACK/NAK builder and `Dhcpv4ServerConfiguration` — build a DHCP server with a handful of lines.
- **New:** `Dhcpv4MessageType` / `Dhcpv6MessageType` enums and `getOptions()` / `getMessageType()` helpers.
- **Fixed:** `buildPacket()` no longer appends stray padding bytes (`GetBuffer()` → `ToArray()`).
- **Fixed:** DHCPv6 classes are now `public` and encode option code/length correctly (2-octet big-endian).
- **Fixed:** more robust option parsing (handles Pad/End, guards against truncated/oversized input).
- Added an xUnit test suite and a runnable Linux DHCP server sample.
- Removed the legacy, Windows-only Visual Studio example projects and the vendored `packages/` folder.

Older DhcpDotNet 2.x releases are available on [NuGet](https://www.nuget.org/packages/DhcpDotNet/).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and pull requests are welcome.

## License

DhcpDotNet is released under the [MIT License](LICENSE).

## Author

Created and maintained by [MaxlLuky](https://github.com/maxlluky).
