# DhcpDotNet
 DHCP packet implemented with C#. Build DHCP-packages with nearly all possibilities
 
 ## Packet interpretation
 the following dhcp messages can be created:
- DHCPDISCOVER
- DHCPOFFER
- DHCPREQUEST
- DHCPACK
- DHCPNAK
- DHCPDECLINE
- DHCPRELEASE
- DHCPINFORM
    
 the following options can be created for a DHCP packet: 
- DhcpMessageType
- ClientIdentifier
- RequestedIpAddress
- DhcpServerIdentifier
- IpAddressLeaseTime
- RenewalTimeValue
- RebindingTimeValue
- SubnetMask
- Router
- DomainNameServer
- DomainName
- Hostname
- ClientFullyQualifiedDomainName
- VendorClassIdentifier
- ParameterRequestList
- BroadcastAddress
- NetworkTimeProtocolServers
- PCPServer

## Examples
See the <a href="/DhcpDotNet/Examples/">Examples</a> folder for a range of examples using DhcpDotNet

## Usage
Example of a DHCP Discover package. The payload can be sent with a UdpClient or socket.
```csharp
private static byte[] buildDhcpPayload()
{
    DhcpOption dhcpServerIdentifierOption = new DhcpOption()
    {
        optionId = dhcpOptionIds.DhcpMessageType,
        optionLength = new byte[] { 0x01 },
        optionValue = new byte[] { 0x01 },
    };


    DhcpPacket dhcpDiscoveryPacket = new DhcpPacket()
    {
        transactionID = new byte[] { 0x00, 0x00, 0x00, 0x00 },
        dhcpOptions = dhcpServerIdentifierOption.buildDhcpOption().ToArray(),
    };

    return dhcpDiscoveryPacket.buildPacket();
}
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
