using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DhcpDotNet
{
    /// <summary>
    /// Represents a DHCPv4 message and provides methods to build it into a byte array or to parse
    /// a received UDP payload back into its fields. See RFC 2131 for detailed information:
    /// https://tools.ietf.org/html/rfc2131
    /// </summary>
    public class Dhcpv4Packet
    {
        /// <summary>
        /// Message op code / message type. 1 = BOOTREQUEST, 2 = BOOTREPLY.
        /// </summary>
        public byte op { get; set; }

        /// <summary>
        /// Hardware address type, see ARP section in "Assigned Numbers" RFC; e.g. '1' = 10mb ethernet.
        /// </summary>
        public byte htype { get; set; } = 0x01;

        /// <summary>
        /// Hardware address length (e.g. '6' for 10mb ethernet).
        /// </summary>
        public byte hlen { get; set; } = 0x06;

        /// <summary>
        /// Client sets to zero, optionally used by relay agents when booting via a relay agent.
        /// </summary>
        public byte hops { get; set; }

        /// <summary>
        /// Transaction ID, a random number chosen by the client, used by the client and server to
        /// associate messages and responses between a client and a server.
        /// </summary>
        public byte[] xid { get; set; } = new byte[4];

        /// <summary>
        /// Filled in by client, seconds elapsed since client began address acquisition or renewal process.
        /// </summary>
        public byte[] secs { get; set; } = new byte[2];

        /// <summary>
        /// Flags (see figure 2 of RFC 2131). The top bit is the broadcast flag.
        /// </summary>
        public byte[] flags { get; set; } = new byte[2];

        /// <summary>
        /// Client IP address; only filled in if client is in BOUND, RENEW or REBINDING state and can
        /// respond to ARP requests.
        /// </summary>
        public byte[] ciaddr { get; set; } = new byte[4];

        /// <summary>
        /// 'your' (client) IP address.
        /// </summary>
        public byte[] yiaddr { get; set; } = new byte[4];

        /// <summary>
        /// IP address of next server to use in bootstrap; returned in DHCPOFFER, DHCPACK by server.
        /// </summary>
        public byte[] siaddr { get; set; } = new byte[4];

        /// <summary>
        /// Relay agent IP address, used in booting via a relay agent.
        /// </summary>
        public byte[] giaddr { get; set; } = new byte[4];

        /// <summary>
        /// Client hardware address.
        /// </summary>
        public byte[] chaddr { get; set; } = new byte[6];

        /// <summary>
        /// If your chaddr is not 6 bytes long, use this to pad the chaddr field to its full size of 16 bytes.
        /// </summary>
        public byte[] chaddrPadding { get; set; } = new byte[10];

        /// <summary>
        /// Optional server host name, null terminated string.
        /// </summary>
        public byte[] sname { get; set; } = new byte[64];

        /// <summary>
        /// Boot file name, null terminated string; "generic" name or null in DHCPDISCOVER, fully
        /// qualified directory-path name in DHCPOFFER.
        /// </summary>
        public byte[] file { get; set; } = new byte[128];

        /// <summary>
        /// Magic cookie that identifies the payload as DHCP instead of plain BOOTP.
        /// </summary>
        public byte[] magicCookie { get; set; } = new byte[4] { 0x63, 0x82, 0x53, 0x63 };

        /// <summary>
        /// DHCP parameters and options (described in RFC 2132). Build them with <see cref="Dhcpv4Option"/>.
        /// </summary>
        public byte[] dhcpOptions { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Defines the end of the DHCP options section. Default: 0xff (255).
        /// </summary>
        public byte[] end { get; set; } = new byte[1] { 0xff };

        /// <summary>
        /// Creates a byte array in the form of a DHCPv4 payload, which can be sent via a UDP datagram.
        /// </summary>
        public byte[] buildPacket()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(op);
                binaryWriter.Write(htype);
                binaryWriter.Write(hlen);
                binaryWriter.Write(hops);
                binaryWriter.Write(xid);
                binaryWriter.Write(secs);
                binaryWriter.Write(flags);
                binaryWriter.Write(ciaddr);
                binaryWriter.Write(yiaddr);
                binaryWriter.Write(siaddr);
                binaryWriter.Write(giaddr);
                binaryWriter.Write(chaddr);
                binaryWriter.Write(chaddrPadding);
                binaryWriter.Write(sname);
                binaryWriter.Write(file);
                binaryWriter.Write(magicCookie);
                binaryWriter.Write(dhcpOptions);
                binaryWriter.Write(end);
                binaryWriter.Flush();

                // ToArray() returns exactly the written bytes; GetBuffer() would include the
                // MemoryStream's trailing capacity as stray padding.
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Parses a raw DHCPv4 payload. The return value indicates whether the process was successful.
        /// </summary>
        /// <param name="pPayload">The entire UDP payload.</param>
        public bool parsePacket(byte[] pPayload)
        {
            if (pPayload == null || pPayload.Length < 240)
            {
                // A valid DHCP payload has a 236-byte fixed header plus the 4-byte magic cookie.
                return false;
            }

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(pPayload))
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    op = binaryReader.ReadByte();
                    htype = binaryReader.ReadByte();
                    hlen = binaryReader.ReadByte();
                    hops = binaryReader.ReadByte();
                    xid = binaryReader.ReadBytes(4);
                    secs = binaryReader.ReadBytes(2);
                    flags = binaryReader.ReadBytes(2);
                    ciaddr = binaryReader.ReadBytes(4);
                    yiaddr = binaryReader.ReadBytes(4);
                    siaddr = binaryReader.ReadBytes(4);
                    giaddr = binaryReader.ReadBytes(4);

                    // chaddr is a fixed 16-byte field: hlen bytes of address plus padding.
                    int hardwareLength = Math.Min(Convert.ToInt32(hlen), 16);
                    chaddr = binaryReader.ReadBytes(hardwareLength);
                    chaddrPadding = binaryReader.ReadBytes(16 - hardwareLength);

                    sname = binaryReader.ReadBytes(64);
                    file = binaryReader.ReadBytes(128);
                    magicCookie = binaryReader.ReadBytes(4);
                    dhcpOptions = binaryReader.ReadBytes(pPayload.Length - Convert.ToInt32(binaryReader.BaseStream.Position));
                }

                return true;
            }
            catch (Exception eX)
            {
                Debug.WriteLine("DhcpDotNet-Exception: " + eX.Message);
            }

            return false;
        }

        /// <summary>
        /// Parses and returns the list of DHCP options contained in this packet's <see cref="dhcpOptions"/>.
        /// </summary>
        public List<Dhcpv4Option> getOptions() => new Dhcpv4Option().parseDhcpOptions(dhcpOptions);

        /// <summary>
        /// Returns the DHCP message type carried in option 53, or <c>null</c> if it is not present.
        /// </summary>
        public Dhcpv4MessageType? getMessageType()
        {
            foreach (Dhcpv4Option option in getOptions())
            {
                if (option.optionIdBytes == (byte)Dhcpv4OptionIds.DhcpMessageType &&
                    option.optionValue.Length >= 1)
                {
                    return (Dhcpv4MessageType)option.optionValue[0];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Create a DHCPv4 option, as listed in RFC 2132 and the IANA registry via the <see cref="Dhcpv4OptionIds"/> enum.
    /// </summary>
    public class Dhcpv4Option
    {
        /// <summary>
        /// The DHCPv4 option to be created, by name.
        /// </summary>
        public Dhcpv4OptionIds optionId { get; set; }

        /// <summary>
        /// The option id as a raw byte. Set automatically from <see cref="optionId"/> when building; also
        /// populated when parsing.
        /// </summary>
        public byte optionIdBytes { get; set; }

        /// <summary>
        /// The length of <see cref="optionValue"/>. Set automatically from the value when building.
        /// </summary>
        public byte optionLength { get; set; }

        /// <summary>
        /// The option value, e.g. a subnet mask.
        /// </summary>
        public byte[] optionValue { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Creates the DHCPv4 option as a byte array to be appended to a packet's options section.
        /// </summary>
        public byte[] buildDhcpOption()
        {
            if (Enum.IsDefined(typeof(Dhcpv4OptionIds), optionId))
            {
                optionIdBytes = (byte)optionId;
            }

            // Always derive the length from the actual value so callers cannot desynchronise it.
            optionLength = (byte)optionValue.Length;

            byte[] result = new byte[] { optionIdBytes, optionLength };
            return result.Concat(optionValue).ToArray();
        }

        /// <summary>
        /// Parses the raw options section of a DHCPv4 packet into a list of <see cref="Dhcpv4Option"/>.
        /// Pad options (0x00) are skipped and parsing stops at the End option (0xff) or the end of data.
        /// </summary>
        public List<Dhcpv4Option> parseDhcpOptions(byte[] pPayload)
        {
            List<Dhcpv4Option> dhcpOptionList = new List<Dhcpv4Option>();

            if (pPayload == null || pPayload.Length == 0)
            {
                return dhcpOptionList;
            }

            int index = 0;
            while (index < pPayload.Length)
            {
                byte dhcpOptionId = pPayload[index++];

                if (dhcpOptionId == (byte)Dhcpv4OptionIds.End)
                {
                    break;
                }

                if (dhcpOptionId == (byte)Dhcpv4OptionIds.Padding)
                {
                    // Pad option has no length or value.
                    continue;
                }

                if (index >= pPayload.Length)
                {
                    break;
                }

                byte dhcpOptionValueLength = pPayload[index++];

                if (index + dhcpOptionValueLength > pPayload.Length)
                {
                    // Truncated option; stop rather than reading past the buffer.
                    break;
                }

                byte[] dhcpOptionValue = new byte[dhcpOptionValueLength];
                Array.Copy(pPayload, index, dhcpOptionValue, 0, dhcpOptionValueLength);
                index += dhcpOptionValueLength;

                dhcpOptionList.Add(new Dhcpv4Option
                {
                    optionIdBytes = dhcpOptionId,
                    optionId = Enum.IsDefined(typeof(Dhcpv4OptionIds), dhcpOptionId)
                        ? (Dhcpv4OptionIds)dhcpOptionId
                        : default,
                    optionLength = dhcpOptionValueLength,
                    optionValue = dhcpOptionValue,
                });
            }

            return dhcpOptionList;
        }
    }

    /// <summary>
    /// DHCP message types as carried in option 53 (DhcpMessageType). See RFC 2131 / RFC 3203.
    /// </summary>
    public enum Dhcpv4MessageType : byte
    {
        Discover = 1,
        Offer = 2,
        Request = 3,
        Decline = 4,
        Ack = 5,
        Nak = 6,
        Release = 7,
        Inform = 8,
        ForceRenew = 9,
        LeaseQuery = 10,
        LeaseUnassigned = 11,
        LeaseUnknown = 12,
        LeaseActive = 13,
    }

    /// <summary>
    /// DHCPv4 option ids as listed in RFC 2132 and the IANA registry.
    /// </summary>
    public enum Dhcpv4OptionIds : byte
    {
        // BOOTP Vendor Information Extensions
        Padding = 0,
        Subnetmask = 1,
        TimeOffset = 2,
        Router = 3,
        TimeServer = 4,
        NameServer = 5,
        DomainNameServer = 6,
        LogServer = 7,
        CookieServer = 8,
        LprServer = 9,
        ImpressServer = 10,
        ResourceLocationServer = 11,
        HostName = 12,
        BootFileSize = 13,
        MeritDumpFile = 14,
        DomainName = 15,
        SwapServer = 16,
        RootPath = 17,
        ExtensionsPath = 18,

        // IP layer parameters per host
        IpForwardingEnableDisable = 19,
        NonLocalSourceRoutingEnableDisable = 20,
        PolicyFilter = 21,
        MaximumDatagramReassemblySize = 22,
        DefaultIpTimeToLive = 23,
        PathMtuAgingTimeout = 24,
        PathMtuPlateauTable = 25,

        // IP Layer Parameters per Interface
        InterfaceMtu = 26,
        AllSubnetsAreLocal = 27,
        BroadcastAddress = 28,
        PerformMaskDiscovery = 29,
        MaskSupplier = 30,
        PerformRouterDiscovery = 31,
        RouterSolicitationAddress = 32,
        StaticRoute = 33,

        // Link layer parameters per interface
        TrailerEncapsulationOption = 34,
        ArpCacheTimeout = 35,
        EthernetEncapsulation = 36,

        // TCP parameters
        TcpDefaultTTL = 37,
        TcpKeepaliveInterval = 38,
        TcpKeepaliveGarbage = 39,

        // Application and service parameters
        NetworkInformationServiceDomain = 40,
        NetworkInformationServers = 41,
        NetworkTimeProtocolServers = 42,
        VendorSpecificInformation = 43,
        NetBiosOverTcpIpNameServer = 44,
        NetBiosOverTcpIpDatagramDistributionServer = 45,
        NetBiosOverTcpIpNodeType = 46,
        NetBIOSOverTcpIpScope = 47,
        XWindowSystemFontServer = 48,
        XWindowSystemDisplayManager = 49,
        NetworkInformationServicePlusDomain = 64,
        NetworkInformationServicePlusServers = 65,
        MobileIPHomeAgent = 68,
        SimpleMailTransferProtocolServer = 69,
        PostOfficeProtocolServer = 70,
        NetworkNewsTransferProtocolServer = 71,
        DefaultWorldWideWebServer = 72,
        DefaultFingerProtocolServer = 73,
        DefaultInternetRelayChatServer = 74,
        StreetTalkServer = 75,
        StreetTalkDirectoryAssistanceServer = 76,

        // DHCP extensions
        RequestedIpAddress = 50,
        IpAddressLeaseTime = 51,
        OptionOverload = 52,
        DhcpMessageType = 53,
        ServerIdentifier = 54,
        ParameterRequestList = 55,
        Message = 56,
        MaximumDhcpMessageSize = 57,
        RenewalTimeValue = 58,
        RebindingTimeValue = 59,
        VendorClassIdentifier = 60,
        ClientIdentifier = 61,
        TftpServerName = 66,
        BootfileName = 67,
        ClientFullyQualifiedDomainName = 81,
        DomainSearch = 119,
        ClasslessStaticRoute = 121,

        // BOOTP Vendor Information Extensions
        End = 255,
    }
}
