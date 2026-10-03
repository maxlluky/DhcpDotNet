using System.Linq;
using System.Net;
using DhcpDotNet;
using Xunit;

namespace DhcpDotNet.Tests
{
    public class Dhcpv4Tests
    {
        [Fact]
        public void BuildPacket_HasNoTrailingPadding()
        {
            // Regression test for the old GetBuffer() bug that appended stray zero bytes.
            var option = new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.DhcpMessageType,
                optionValue = new byte[] { (byte)Dhcpv4MessageType.Discover },
            }.buildDhcpOption();

            var packet = new Dhcpv4Packet
            {
                op = 0x01,
                xid = new byte[] { 0x01, 0x02, 0x03, 0x04 },
                dhcpOptions = option,
            };

            byte[] bytes = packet.buildPacket();

            // 236 fixed header + 4 magic cookie + 3 option bytes + 1 end byte = 244
            Assert.Equal(244, bytes.Length);
            Assert.Equal(0xff, bytes[^1]);
        }

        [Fact]
        public void BuildThenParse_RoundTripsFields()
        {
            var packet = new Dhcpv4Packet
            {
                op = 0x01,
                htype = 0x01,
                hlen = 0x06,
                xid = new byte[] { 0xde, 0xad, 0xbe, 0xef },
                chaddr = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 },
                dhcpOptions = new Dhcpv4Option
                {
                    optionId = Dhcpv4OptionIds.DhcpMessageType,
                    optionValue = new byte[] { (byte)Dhcpv4MessageType.Request },
                }.buildDhcpOption(),
            };

            byte[] bytes = packet.buildPacket();

            var parsed = new Dhcpv4Packet();
            Assert.True(parsed.parsePacket(bytes));
            Assert.Equal(0x01, parsed.op);
            Assert.Equal(new byte[] { 0xde, 0xad, 0xbe, 0xef }, parsed.xid);
            Assert.Equal(new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 }, parsed.chaddr);
            Assert.Equal(Dhcpv4MessageType.Request, parsed.getMessageType());
        }

        [Fact]
        public void ParsePacket_RejectsTooShortPayload()
        {
            var parsed = new Dhcpv4Packet();
            Assert.False(parsed.parsePacket(new byte[] { 0x01, 0x02, 0x03 }));
        }

        [Fact]
        public void ParseOptions_SkipsPadAndStopsAtEnd()
        {
            // Pad, MessageType=Ack, Pad, End, trailing garbage that must be ignored.
            byte[] raw = { 0x00, 53, 0x01, 0x05, 0x00, 0xff, 0xAA, 0xBB };

            var options = new Dhcpv4Option().parseDhcpOptions(raw);

            Assert.Single(options);
            Assert.Equal((byte)Dhcpv4OptionIds.DhcpMessageType, options[0].optionIdBytes);
            Assert.Equal(new byte[] { 0x05 }, options[0].optionValue);
        }

        [Fact]
        public void BuildDhcpOption_DerivesLengthFromValue()
        {
            var option = new Dhcpv4Option
            {
                optionId = Dhcpv4OptionIds.Subnetmask,
                optionValue = IPAddress.Parse("255.255.255.0").GetAddressBytes(),
            };

            byte[] bytes = option.buildDhcpOption();

            Assert.Equal((byte)Dhcpv4OptionIds.Subnetmask, bytes[0]);
            Assert.Equal(4, bytes[1]);
            Assert.Equal(new byte[] { 255, 255, 255, 0 }, bytes.Skip(2).ToArray());
        }

        [Fact]
        public void BuildDhcpOption_SupportsCodesMissingFromEnum()
        {
            var option = new Dhcpv4Option
            {
                optionIdBytes = 252, // WPAD, not part of Dhcpv4OptionIds
                optionValue = new byte[] { 0x41 },
            };

            Assert.Equal(new byte[] { 252, 1, 0x41 }, option.buildDhcpOption());
        }

        [Fact]
        public void ParsedUnknownOption_RebuildsWithItsOwnCode()
        {
            var option = new Dhcpv4Option().parseDhcpOptions(new byte[] { 252, 1, 0x41, 255 }).Single();

            Assert.Equal(252, option.optionIdBytes);
            Assert.Equal((Dhcpv4OptionIds)252, option.optionId);
            Assert.Equal(new byte[] { 252, 1, 0x41 }, option.buildDhcpOption());
        }

        [Fact]
        public void OptionIdAndOptionIdBytes_StayInSync()
        {
            var option = new Dhcpv4Option { optionId = Dhcpv4OptionIds.Router };
            Assert.Equal(3, option.optionIdBytes);

            option.optionIdBytes = 6;
            Assert.Equal(Dhcpv4OptionIds.DomainNameServer, option.optionId);
        }

        [Fact]
        public void Reply_CreateOffer_EchoesXidAndSetsYiaddr()
        {
            var request = new Dhcpv4Packet
            {
                op = 0x01,
                xid = new byte[] { 0x11, 0x22, 0x33, 0x44 },
                chaddr = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 },
            };

            var config = new Dhcpv4ServerConfiguration
            {
                ServerIdentifier = IPAddress.Parse("192.168.50.1"),
                SubnetMask = IPAddress.Parse("255.255.255.0"),
                Router = IPAddress.Parse("192.168.50.1"),
            };
            config.DnsServers.Add(IPAddress.Parse("1.1.1.1"));

            Dhcpv4Packet offer = Dhcpv4Reply.CreateOffer(request, IPAddress.Parse("192.168.50.100"), config);

            Assert.Equal(0x02, offer.op); // BOOTREPLY
            Assert.Equal(request.xid, offer.xid);
            Assert.Equal(IPAddress.Parse("192.168.50.100").GetAddressBytes(), offer.yiaddr);
            Assert.Equal(Dhcpv4MessageType.Offer, offer.getMessageType());
        }
    }

    public class Dhcpv6Tests
    {
        [Fact]
        public void BuildDhcpOption_UsesBigEndianCodeAndLength()
        {
            var option = new Dhcpv6Option
            {
                optionId = Dhcpv6OptionIds.ELAPSED_TIME, // code 8
                optionValue = new byte[] { 0x00, 0x00 },
            };

            byte[] bytes = option.buildDhcpOption();

            // code (2 bytes BE) + length (2 bytes BE) + value
            Assert.Equal(new byte[] { 0x00, 0x08, 0x00, 0x02, 0x00, 0x00 }, bytes);
        }

        [Fact]
        public void BuildThenParse_RoundTripsOptions()
        {
            byte[] opt = new Dhcpv6Option
            {
                optionId = Dhcpv6OptionIds.PREFERENCE, // code 7
                optionValue = new byte[] { 0xff },
            }.buildDhcpOption();

            var packet = new Dhcpv6Packet
            {
                msgtype = (byte)Dhcpv6MessageType.ADVERTISE,
                transactionid = new byte[] { 0x0a, 0x0b, 0x0c },
                options = opt,
            };

            byte[] bytes = packet.buildPacket();

            var parsed = new Dhcpv6Packet();
            Assert.True(parsed.parsePacket(bytes));
            Assert.Equal((byte)Dhcpv6MessageType.ADVERTISE, parsed.msgtype);
            Assert.Equal(new byte[] { 0x0a, 0x0b, 0x0c }, parsed.transactionid);

            var options = parsed.getOptions();
            Assert.Single(options);
            Assert.Equal(Dhcpv6OptionIds.PREFERENCE, options[0].optionId);
            Assert.Equal(new byte[] { 0xff }, options[0].optionValue);
        }
    }

    public class Dhcpv4LeasePoolTests
    {
        [Fact]
        public void Acquire_GivesStableAddressPerClient()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.12"),
                System.TimeSpan.FromMinutes(10));

            byte[] client = { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55 };

            IPAddress? first = pool.Acquire(client);
            IPAddress? second = pool.Acquire(client);

            Assert.NotNull(first);
            Assert.Equal(first, second); // renewals keep the same address
        }

        [Fact]
        public void Acquire_DifferentClientsGetDifferentAddresses()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.12"),
                System.TimeSpan.FromMinutes(10));

            IPAddress? a = pool.Acquire(new byte[] { 0x01 });
            IPAddress? b = pool.Acquire(new byte[] { 0x02 });

            Assert.NotNull(a);
            Assert.NotNull(b);
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Acquire_HonoursRequestedAddressWhenFree()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.20"),
                System.TimeSpan.FromMinutes(10));

            IPAddress? granted = pool.Acquire(new byte[] { 0x01 }, IPAddress.Parse("10.0.0.15"));

            Assert.Equal(IPAddress.Parse("10.0.0.15"), granted);
        }

        [Fact]
        public void Acquire_ReturnsNullWhenExhausted()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.11"),
                System.TimeSpan.FromMinutes(10));

            Assert.NotNull(pool.Acquire(new byte[] { 0x01 }));
            Assert.NotNull(pool.Acquire(new byte[] { 0x02 }));
            Assert.Null(pool.Acquire(new byte[] { 0x03 })); // only two addresses in range
        }

        [Fact]
        public void Release_FreesAddressForReuse()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.10"),
                System.TimeSpan.FromMinutes(10));

            byte[] client1 = { 0x01 };
            IPAddress? a = pool.Acquire(client1);
            Assert.NotNull(a);
            Assert.Null(pool.Acquire(new byte[] { 0x02 })); // exhausted

            Assert.True(pool.Release(client1));
            IPAddress? reused = pool.Acquire(new byte[] { 0x02 });
            Assert.Equal(a, reused);
        }

        [Fact]
        public void Offer_ReservationEndsAfterOfferTime()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.10"),
                System.TimeSpan.FromHours(12));
            pool.OfferTime = System.TimeSpan.Zero; // the offer expires immediately

            Assert.Equal(IPAddress.Parse("10.0.0.10"), pool.Offer(new byte[] { 0x01 }));
            Assert.Equal(IPAddress.Parse("10.0.0.10"), pool.Offer(new byte[] { 0x02 }));
        }

        [Fact]
        public void Offer_IsShortAndAcquireTurnsItIntoAFullLease()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.20"),
                System.TimeSpan.FromHours(12));
            byte[] client = { 0x01 };

            IPAddress? offered = pool.Offer(client);
            Assert.True(pool.GetLease(client)!.ExpiresAt < System.DateTimeOffset.UtcNow.AddMinutes(2));

            Assert.Equal(offered, pool.Acquire(client));
            Assert.True(pool.GetLease(client)!.ExpiresAt > System.DateTimeOffset.UtcNow.AddHours(11));
        }

        [Fact]
        public void Offer_NeverShortensAnExistingLease()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.20"),
                System.TimeSpan.FromHours(12));
            byte[] client = { 0x01 };

            IPAddress? leased = pool.Acquire(client);
            Assert.Equal(leased, pool.Offer(client));
            Assert.True(pool.GetLease(client)!.ExpiresAt > System.DateTimeOffset.UtcNow.AddHours(11));
        }

        [Fact]
        public void GetLease_ReturnsNullForUnknownClient()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.20"),
                System.TimeSpan.FromHours(12));

            Assert.Null(pool.GetLease(new byte[] { 0x01 }));
            Assert.Empty(pool.GetActiveLeases());
        }

        [Fact]
        public void Decline_BlocksTheAddress()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.11"),
                System.TimeSpan.FromHours(12));
            byte[] client = { 0x01 };
            IPAddress leased = pool.Acquire(client)!;

            Assert.False(pool.Decline(client, IPAddress.Parse("10.0.0.11"))); // not the client's address
            Assert.True(pool.Decline(client, leased));

            Assert.Null(pool.GetLease(client));
            Assert.Equal(IPAddress.Parse("10.0.0.11"), pool.Acquire(client));
            Assert.Null(pool.Acquire(new byte[] { 0x02 }, leased)); // blocked, and nothing else left
        }

        [Fact]
        public void Exclude_KeepsAddressOutOfThePool()
        {
            var pool = new Dhcpv4LeasePool(
                IPAddress.Parse("10.0.0.10"),
                IPAddress.Parse("10.0.0.11"),
                System.TimeSpan.FromHours(12));
            pool.Exclude(IPAddress.Parse("10.0.0.10"));

            Assert.Equal(IPAddress.Parse("10.0.0.11"), pool.Acquire(new byte[] { 0x01 }, IPAddress.Parse("10.0.0.10")));
            Assert.Null(pool.Offer(new byte[] { 0x02 }));
        }
    }
}
