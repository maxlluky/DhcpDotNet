<a>
    <img src="DhcpDotNet/logo.png" alt="DhcpDotNet" align="right" height="60" />
</a>

# DhcpDotNet
DHCP packet implemented with C#. Build DHCP-packages with nearly all possibilities
 
## Packet interpretation
the following dhcp messages can be created:
```
- DHCPDISCOVER
- DHCPOFFER
- DHCPREQUEST
- DHCPACK
- DHCPNAK
- DHCPDECLINE
- DHCPRELEASE
- DHCPINFORM
```

The following tables list the available DHCP options, as listed in RFC 2132 and IANA registry:
```
// BOOTP Vendor Information Extensions
Padding = 0,
Hostname = 1,
TimeOffset = 2,
Router = 3,
TimeServer = 4,
NameServer = 5,
DomainNameServer = 6,
LogServer = 7,
CookieServer = 8,
LprServer = 9,
ImpressServer = 10,
ResourceLocationServer = 11,
HostName = 12,
BootFileSize = 13,
MeritDumpFile = 14,
DomainName = 15,
SwapServer = 16,
RootPath = 17,
ExtensionsPath = 18,

// IP layer parameters per host
IpForwardingEnableDisable = 19,
NonLocalSourceRoutingEnableDisable = 20,
PolicyFilter = 21,
MaximumDatagramReassemblySize = 22,
DefaultIpTimeToLive = 23,
PathMtuAgingTimeout = 24,
PathMtuPlateauTable = 25,

// IP Layer Parameters per Interface
InterfaceMtu = 26,
AllSubnetsAreLocal = 27,
BroadcastAddress = 28,
PerformMaskDiscovery = 29,
MaskSupplier = 30,
PerformRouterDiscovery = 31,
RouterSolicitationAddress = 32,
StaticRoute = 33,

// Link layer parameters per interface
TrailerEncapsulationOption = 34,
ArpCacheTimeout = 35,
EthernetEncapsulation = 36,

// TCP parameters
TcpDefaultTTL = 37,
TcpKeepaliveInterval = 38,
TcpKeepaliveGarbage = 39,

// Application and service parameters
NetworkInformationServiceDomain = 40,
NetworkInformationServers = 41,
NetworkTimeProtocolServers = 42,
VendorSpecificInformation = 43,
NetBiosOverTcpIpNameServer = 44,
NetBiosOverTcpIpDatagramDistributionServer = 45,
NetBiosOverTcpIpNodeType = 46,
NetBIOSOverTcpIpScope = 47,
XWindowSystemFontServer = 48,
XWindowSystemDisplayManager = 49,
NetworkInformationServicePlusDomain = 64,
NetworkInformationServicePlusServers = 65,
MobileIPHomeAgent = 68,
SimpleMailTransferProtocolServer = 69,
PostOfficeProtocolServer = 70,
NetworkNewsTransferProtocolServer = 71,
DefaultWorldWideWebServer = 72,
DefaultFingerProtocolServer = 73,
DefaultInternetRelayChatServer = 74,
StreetTalkServer = 75,
StreetTalkDirectoryAssistanceServer = 76,

// DHCP extensions
RequestedIpAddress = 50,
IpAddressLeaseTime = 51,
OptionOverload = 52,
DhcpMessageType = 53,
ServerIdentifier = 54,
ParameterRequestList = 55,
Message = 56,
MaximumDhcpMessageSize = 57,
RenewalTimeValue = 58,
RebindingTimeValue = 59,
VendorClassIdentifier = 60,
ClientIdentifier = 61,
TftpServerName = 66,
BootfileName = 67,

// BOOTP Vendor Information Extensions
End = 255,
```

## Examples
See the <a href="/DhcpDotNet/Examples/">Examples</a> folder for a range of examples using DhcpDotNet

## Usage
Example of a DHCP Discover package. The payload can be sent with a UdpClient or socket.
```csharp
Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
IPAddress serverAddr = IPAddress.Parse("192.168.2.1");
IPEndPoint endPoint = new IPEndPoint(serverAddr, 67);

DhcpOption dhcpServerIdentifierOption = new DhcpOption()
{
    optionId = dhcpOptionIds.DhcpMessageType,
    optionLength = new byte[] { 0x01 },
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

## NuGet
Package Manager
```
PM> Install-Package DhcpDotNet -Version 1.0.7
```

.NET CLI
```
> dotnet add package DhcpDotNet --version 1.0.7
```
<a href="https://www.nuget.org/packages/DhcpDotNet/">NuGet-Page</a>

## Author
This developer and the copyright holder of this library is <a href="https://github.com/Marschall-dev">Marschall-dev</a>
