using System;
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

            DhcpRequestPacket dhcpRequestPacket = new DhcpRequestPacket()
            {
                transactionID = _tempTransactionId,
                dhcpServerId = IPAddress.Parse("192.168.178.1").GetAddressBytes(),
            };

            return dhcpRequestPacket.buildPacket();
        }
    }
}
