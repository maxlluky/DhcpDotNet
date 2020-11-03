using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace DHCP_Request
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse("192.168.178.1");
            IPEndPoint endPoint = new IPEndPoint(serverAddr, 67);

            byte[] send_buffer = buildDhcpPayload();

            if (sock.SendTo(send_buffer, endPoint) != 0)
            {
                Console.WriteLine("Send successful. See Wireshark output: DHCP Discovery with option Hostname: Csharp");
            }

            Console.Read();
        }

        private static byte[] buildDhcpPayload()
        {
            // Create optional payload bytes that can be added to the main payload.
            DhcpOption dhcpServerIdentifierOption = new DhcpOption()
            {
                optionId = dhcpOptionIds.DhcpMessageType,
                optionLength = new byte[] { 0x01 },
                optionValue = new byte[] { 0x03 },
            };

            DhcpOption dhcpRequestedIpAddressOption = new DhcpOption()
            {
                optionId = dhcpOptionIds.RequestedIpAddress,
                optionLength = new byte[] { 0x04 },
                optionValue = IPAddress.Parse("192.168.178.101").GetAddressBytes(),
            };

            // Create the main payload of the dhcp-packet and add the options to it.
            DhcpPacket dhcpDiscoveryPacket = new DhcpPacket()
            {
                transactionID = new byte[] { 0x00, 0x00, 0x00, 0x00 },
                dhcpOptions = dhcpServerIdentifierOption.buildDhcpOption().Concat(dhcpRequestedIpAddressOption.buildDhcpOption()).ToArray(),
            };

            return dhcpDiscoveryPacket.buildPacket();
        }
    }
}
