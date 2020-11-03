# DhcpDotNet
 DHCP packet implemented with C#

## Examples
See the <a href="/DhcpDotNet/Examples/">Examples</a> folder for a range of examples using SharpPcap

## NuGet
<a href="https://www.nuget.org/packages/DhcpDotNet/">NuGet-Page</a>

## Usage
Example of a DHCP Discover package. The payload can be sent with a UdpClient or socket.
```csharp
        private static byte[] buildDhcpPayload()
        {
            // Create optional payload bytes that can be added to the main payload.
            DhcpOption dhcpServerIdentifierOption = new DhcpOption()
            {
                optionId = dhcpOptionIds.DhcpMessageType,
                optionLength = new byte[] { 0x01 },
                optionValue = new byte[] { 0x01 },
            };

            // Create the main payload of the dhcp-packet and add the options to it.
            DhcpPacket dhcpDiscoveryPacket = new DhcpPacket()
            {
                transactionID = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                dhcpOptions = dhcpServerIdentifierOption.buildDhcpOption().ToArray(),
            };

            return dhcpDiscoveryPacket.buildPacket();
        }
```
