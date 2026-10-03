using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
            Assert.Equal(300, result.Buffer.Length); // padded to the BOOTP minimum

            await server.StopAsync();
        }

        [Fact]
        public async Task RunAsync_ReturnsWhenTokenIsCancelled()
        {
            using var server = new Dhcpv4Server(IPAddress.Loopback, GetFreeUdpPort());
            using var cts = new CancellationTokenSource();

            Task run = server.RunAsync(cts.Token);
            await Task.Delay(200);
            Assert.True(server.IsRunning);

            cts.Cancel();

            Assert.Same(run, await Task.WhenAny(run, Task.Delay(TimeSpan.FromSeconds(5))));
            await run;
            Assert.False(server.IsRunning);
        }

        [Fact]
        public async Task Server_CanBeStoppedAndStartedAgain()
        {
            using var server = new Dhcpv4Server(IPAddress.Loopback, GetFreeUdpPort());

            server.Start();
            await server.StopAsync();
            await server.StopAsync(); // a second stop is harmless
            Assert.False(server.IsRunning);

            server.Start();
            Assert.True(server.IsRunning);
            await server.StopAsync();
        }

        [Fact]
        public void ForInterface_RejectsUnknownInterface()
        {
            Assert.Throws<ArgumentException>(() => Dhcpv4Server.ForInterface("no-such-interface-42"));
        }

        [Fact]
        public async Task ForInterface_ServesThatInterface()
        {
            NetworkInterface loopback = NetworkInterface.GetAllNetworkInterfaces()
                .First(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Loopback);
            int port = GetFreeUdpPort();
            using var server = Dhcpv4Server.ForInterface(loopback, port);

            var received = new TaskCompletionSource<Dhcpv4MessageType?>(TaskCreationOptions.RunContinuationsAsynchronously);
            server.PacketReceived += (_, e) => received.TrySetResult(e.MessageType);
            server.Start();

            using var client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            byte[] inform = new Dhcpv4Packet
            {
                op = 0x01,
                chaddr = new byte[] { 0x00, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE },
                dhcpOptions = new Dhcpv4Option
                {
                    optionId = Dhcpv4OptionIds.DhcpMessageType,
                    optionValue = new byte[] { (byte)Dhcpv4MessageType.Inform },
                }.buildDhcpOption(),
            }.buildPacket();
            await client.SendAsync(inform, inform.Length, new IPEndPoint(IPAddress.Loopback, port));

            Assert.Same(received.Task, await Task.WhenAny(received.Task, Task.Delay(TimeSpan.FromSeconds(5))));
            Assert.Equal(Dhcpv4MessageType.Inform, await received.Task);

            await server.StopAsync();
        }

        private static int GetFreeUdpPort()
        {
            using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
            return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
        }
    }
}
