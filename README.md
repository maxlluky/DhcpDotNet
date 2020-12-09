<a>
    <img src="DhcpDotNet/logo.png" alt="DhcpDotNet" align="right" height="60" />
</a>

# DhcpDotNet
DHCPv4 and DHCPv6 packet implemented with C#. Build DHCP-packages with nearly all possibilities.
DhcpDotNet allows the programming of a DHCPv4/DHCPv6 server or client with full controll. DhcpDotNet was created according to the specifications of <a href="https://www.iana.org/assignments/bootp-dhcp-parameters/bootp-dhcp-parameters.xhtml">IANA</a> and RFC 2131, 4388, 1531, 8415 and 3315.

- Build DHCP Packets
- Send DHCP Packets (via UdpClient, Socket (advanced: sharppcap or Pcap.Net)
- Receive DHCP Packets (see sending)
- Parse DHCP Packets
- Read parsed Packets

## Become a contributor
If you want to help improve the project, you can read <a href="CONTRIBUTING.md">this<a/>. If you would like to be part of the project, please contact us <a href="mailto:maxarttm@gmail.com">here</a>

## Example Projects
See the <a href="/DhcpDotNet/Examples/">Examples</a> folder for a range of examples using DhcpDotNet.<br>
A DHCPv4 server under C# will soon appear under the name DhcpSharp.

## Usage Example
Example of a DHCPv4 Discover package. The payload can be sent with a UdpClient or socket. (or using Sharppcap or Pcap.Net)
```csharp
Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
IPAddress serverAddr = IPAddress.Parse("192.168.2.1");
IPEndPoint endPoint = new IPEndPoint(serverAddr, 67);

DhcpOption dhcpMessageTypeOption = new DhcpOption()
{
    optionId = dhcpOptionIds.DhcpMessageType,
    optionLength = 0x01,
    optionValue = new byte[] { 0x01 },
};

DhcpPacket dhcpDiscoveryPacket = new DhcpPacket()
{
    transactionID = new byte[] { 0x00, 0x00, 0x00, 0x00 },
    dhcpOptions = dhcpServerIdentifierOption.buildDhcpOption(),
};

byte[] send_buffer = dhcpDiscoveryPacket.buildPacket();
sock.SendTo(send_buffer, endPoint);
```
Please take a look at the DhcpSharp-Project for detailed information.

## NuGet
Package Manager
```
PM> Install-Package DhcpDotNet -Version 2.0.3
```

.NET CLI
```
> dotnet add package DhcpDotNet --version 2.0.3
```
<a href="https://www.nuget.org/packages/DhcpDotNet/">NuGet-Page</a>

## Latest Version and Changelog
Version: 2.0.2

```
V 2.0.3
- Full support for DHCPv6. Fixed several performance down grades. Switch from linq to Binary reader/writer.

V 2.0.2
- Renamed class and methods with "v4". Dhcp with IPv6 comming soon...

V. 2.0.1
- Added dhcpOption parsing support. DhcpDotNet is now able to parse incoming DhcpPackets and their DhcpOptions. You can read each DhcpOption cotained in a DhcpPacket. This allows you to read all information provided by the options above listed.
- Renamed DhcpPacket-Bytes. Each byte or byte-array in a DhcpPacket is now named as in the RFC 2132.
- Added the possibility to define DhcpOptions with an enmu or single byte. This offers more flexibility and simplicity at the same time.
```

## Author
This developer and the copyright holder of this library is <a href="https://github.com/Marschall-dev">Marschall-dev</a>
