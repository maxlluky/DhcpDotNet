# DhcpDotNet
 DHCP packet implemented with C#

## Examples
See the <a href="/DhcpDotNet/Examples/">Examples</a> folder for a range of examples using SharpPcap

## NuGet
<a href="https://www.nuget.org/packages/DhcpDotNet/">NuGet-Page</a>

## Usage

```csharp
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
```
