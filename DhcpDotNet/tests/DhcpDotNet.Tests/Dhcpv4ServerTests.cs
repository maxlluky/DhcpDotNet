using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DhcpDotNet;
using Xunit;

namespace DhcpDotNet.Tests
{
    public class Dhcpv4ServerTests
    {
        [Fact]
        public async Task Server_ReceivesParsesAndReplies()
        {
            // Bind the server to a free loopback port so the test needs no privileges.
            int port = GetFreeUdpPort();
            using var server = new Dhcpv4Server(IPAddress.Loopback, port);

            server.PacketReceived += (_, e) =>
            {
                Assert.Equal(Dhcpv4MessageType.Discover, e.MessageType);

                var config = new Dhcpv4ServerConfiguration
                {
                    ServerIdentifier = IPAddress.Loopback,
                    SubnetMask = IPAddress.Parse("255.255.255.0"),
                };

                e.Reply = Dhcpv4Reply.CreateOffer(e.Packet, IPAddress.Parse("10.1.2.3"), config);
                // Reply straight back to the sender so the loopback test can observe it.
                e.ReplyEndPoint = e.RemoteEndPoint;
            };

            server.Start();

            using var client = new UdpClient();
            client.Client.Bind(new IPEndPoint(IPAddress.Loopback, 0));

            byte[] discover = new Dhcpv4Packet
            {
                op = 0x01,
                xid = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                chaddr = new byte[] { 0x00, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE },
                dhcpOptions = new Dhcpv4Option
                {
                    optionId = Dhcpv4OptionIds.DhcpMessageType,
                    optionValue = new byte[] { (byte)Dhcpv4MessageType.Discover },
                }.buildDhcpOption(),
            }.buildPacket();

            await client.SendAsync(discover, discover.Length, new IPEndPoint(IPAddress.Loopback, port));

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            UdpReceiveResult result = await client.ReceiveAsync(cts.Token);

            var reply = new Dhcpv4Packet();
            Assert.True(reply.parsePacket(result.Buffer));
            Assert.Equal(0x02, reply.op); // BOOTREPLY
            Assert.Equal(Dhcpv4MessageType.Offer, reply.getMessageType());
            Assert.Equal(IPAddress.Parse("10.1.2.3").GetAddressBytes(), reply.yiaddr);

            await server.StopAsync();
        }

        private static int GetFreeUdpPort()
        {
            using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
        }
    }
}
