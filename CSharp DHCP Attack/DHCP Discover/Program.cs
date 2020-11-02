using System;
using System.Net;
using System.Net.Sockets;

namespace DHCP_Discover
{
    class Program
    {        
        static void Main(string[] args)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPAddress serverAddr = IPAddress.Parse("192.168.178.1");

            IPEndPoint endPoint = new IPEndPoint(serverAddr, 67);

            byte[] send_buffer = dhcpPacket();

            if (sock.SendTo(send_buffer, endPoint) != 0)
            {
                Console.WriteLine("Send successful. See Wireshark output: DHCP Discovery with option Hostname: Csharp");
            }

            Console.Read();
        }
       
        private static byte[] dhcpPacket()
        {
            Random rand = new Random();
            byte[] _tempTransactionId = new byte[4];
            rand.NextBytes(_tempTransactionId);

            DhcpDiscoveryPacket dhcpDiscoveryPacket = new DhcpDiscoveryPacket()
            {
                dhcpMessageType = new byte[] { 0x01 },
                transactionID = _tempTransactionId,
            };

            return dhcpDiscoveryPacket.buildPacket();
        }
    }
}
