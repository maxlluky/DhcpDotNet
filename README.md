<a>
    <img src="DhcpDotNet/logo.png" alt="DhcpDotNet" align="right" height="60" />
</a>

# DhcpDotNet
DHCP packet implemented with C#. Build DHCP-packages with nearly all possibilities.
DhcpDotNet allows the programming of a DHCP server, clients with full controller for C#. 

- Build DHCP Packets
- Parse incoming DHCP Packets
- Read parsed Packets

## Become a contributor
If you want to help improve the project, you can read <a href="CONTRIBUTING.md">this<a/>. If you would like to be part of the project, please contact us <a href="mailto:maxarttm@gmail.com">here</a>
 
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

The following tables list the available DHCP options, as listed in <a href="https://tools.ietf.org/html/rfc2132">RFC 2132</a> and IANA registry:
```
// BOOTP Vendor Information Extensions
Padding
Subnetmask
TimeOffset
Router
TimeServer
NameServer
DomainNameServer
LogServer
CookieServer
LprServer
ImpressServer,
ResourceLocationServer
HostName
BootFileSize
MeritDumpFile
DomainName
SwapServer
RootPath
ExtensionsPath

// IP layer parameters per host
IpForwardingEnableDisable
NonLocalSourceRoutingEnableDisable
PolicyFilter
MaximumDatagramReassemblySize
DefaultIpTimeToLive
PathMtuAgingTimeout
PathMtuPlateauTable

// IP Layer Parameters per Interface
InterfaceMtu
AllSubnetsAreLocal
BroadcastAddress
PerformMaskDiscovery
MaskSupplier
PerformRouterDiscovery
RouterSolicitationAddress
StaticRoute

// Link layer parameters per interface
TrailerEncapsulationOption
ArpCacheTimeout
EthernetEncapsulation

// TCP parameters
TcpDefaultTTL
TcpKeepaliveInterval
TcpKeepaliveGarbage

// Application and service parameters
NetworkInformationServiceDomain
NetworkInformationServers
NetworkTimeProtocolServers
VendorSpecificInformation
NetBiosOverTcpIpNameServer
NetBiosOverTcpIpDatagramDistributionServer
NetBiosOverTcpIpNodeType
NetBIOSOverTcpIpScope
XWindowSystemFontServer
XWindowSystemDisplayManager
NetworkInformationServicePlusDomain
NetworkInformationServicePlusServers
MobileIPHomeAgent
SimpleMailTransferProtocolServer
PostOfficeProtocolServer
NetworkNewsTransferProtocolServer
DefaultWorldWideWebServer
DefaultFingerProtocolServer
DefaultInternetRelayChatServer
StreetTalkServer
StreetTalkDirectoryAssistanceServer

// DHCP extensions
RequestedIpAddress
IpAddressLeaseTime
OptionOverload
DhcpMessageType
ServerIdentifier
ParameterRequestList
Message
MaximumDhcpMessageSize
RenewalTimeValue
RebindingTimeValue
VendorClassIdentifier
ClientIdentifier
TftpServerName
BootfileName

// BOOTP Vendor Information Extensions
End
```

## Examples
See the <a href="/DhcpDotNet/Examples/">Examples</a> folder for a range of examples using DhcpDotNet.<br>
A DHCPv4 server under C# will soon appear under the name DhcpSharp

## Usage
Example of a DHCP Discover package. The payload can be sent with a UdpClient or socket.
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

## NuGet
Package Manager
```
PM> Install-Package DhcpDotNet -Version 2.0.0
```

.NET CLI
```
> dotnet add package DhcpDotNet --version 2.0.0
```
<a href="https://www.nuget.org/packages/DhcpDotNet/">NuGet-Page</a>

## Author
This developer and the copyright holder of this library is <a href="https://github.com/Marschall-dev">Marschall-dev</a>
