using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DhcpDotNet
{
    /// <summary>
    /// Creates an empty predefined DHCP packet in the form of a byte array. Please visit RFC 2131 for detaied information.
    /// </summary>
    public class DhcpPacket
    {
        /// <summary>
        /// Message op code / message type. 1 = BOOTREQUEST, 2 = BOOTREPLY
        /// </summary>
        public byte[] op { get; set; } = new byte[1] { 0x01 };

        /// <summary>
        /// Hardware address type, see ARP section in "Assigned Numbers" RFC; e.g., '1' = 10mb ethernet.
        /// </summary>
        public byte[] htype { get; set; } = new byte[1] { 0x01 };

        /// <summary>
        /// Hardware address length (e.g.  '6' for 10mb ethernet).
        /// </summary>
        public byte[] hlen { get; set; } = new byte[1] { 0x06 };

        /// <summary>
        /// Client sets to zero, optionally used by relay agents when booting via a relay agent.
        /// </summary>
        public byte[] hops { get; set; } = new byte[1] { 0x00 };

        /// <summary>
        ///  Transaction ID, a random number chosen by the client, used by the client and server to associate messages and responses between a client and a server.
        /// </summary>
        public byte[] xid { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Filled in by client, seconds elapsed since client began address acquisition or renewal process.
        /// </summary>
        public byte[] secs { get; set; } = new byte[2] { 0x0c, 0x00 };

        /// <summary>
        /// Flags (see figure 2).
        /// </summary>
        public byte[] flags { get; set; } = new byte[2] { 0x00, 0x00 };

        /// <summary>
        /// Client IP address; only filled in if client is in BOUND, RENEW or REBINDING state and can respond to ARP requests.
        /// </summary>
        public byte[] ciaddr { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// 'your' (client) IP address.
        /// </summary>
        public byte[] yiaddr { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// IP address of next server to use in bootstrap; returned in DHCPOFFER, DHCPACK by server.
        /// </summary>
        public byte[] siaddr { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Relay agent IP address, used in booting via a relay agent.
        /// </summary>
        public byte[] giaddr { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Client hardware address.
        /// </summary>
        public byte[] chaddr { get; set; } = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// If your chaddr is not 6 bytes long use this to match the chaddress-size of 16 bytes.
        /// </summary>
        public byte[] chaddrPadding { get; set; } = new byte[10] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Optional server host name, null terminated string.
        /// </summary>
        public byte[] sname { get; set; } = new byte[64] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Boot file name, null terminated string; "generic" name or null in DHCPDISCOVER, fully qualified directory-path name in DHCPOFFER.
        /// </summary>
        public byte[] file { get; set; } = new byte[128] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Defines DHCP, instead of BOOTP
        /// </summary>
        public byte[] magicCookie { get; set; } = new byte[4] { 0x63, 0x82, 0x53, 0x63 };

        /// <summary>
        /// DHCP parameters and options (described in RFC 2132) - The options can be up to 312 bytes long, so a packet up to 576 bytes long can occur. A larger maximum byte count can be 'negotiated' between server and client.
        /// </summary>
        public byte[] dhcpOptions { get; set; } = new byte[] { };

        /// <summary>
        /// Defines the end of the DHCP payload. Default: ff(hex), 255(dec)
        /// </summary>
        public byte[] end { get; set; } = new byte[1] { 0xff };

        /// <summary>
        /// Creates a byte array in the form of a DHCP payload, which can be sent via a UDP datagram. 
        /// </summary>
        /// /// <returns></returns>
        public byte[] buildPacket()
        {
            return op.Concat(htype).Concat(hlen).Concat(hops).Concat(xid).Concat(secs).Concat(flags).Concat(ciaddr).Concat(yiaddr).Concat(siaddr).Concat(giaddr).Concat(chaddr).Concat(chaddrPadding).Concat(sname).Concat(file).Concat(magicCookie).Concat(dhcpOptions).Concat(end).ToArray();
        }

        /// <summary>
        /// Parses a raw-DHCP payload. The return value indicates whether the process was successful. 
        /// </summary>
        /// <param name="pPayload">The entire Udp payload must be transferred</param>
        /// <returns></returns>
        public bool parsePacket(byte[] pPayload)
        {
            try
            {
                op = pPayload.Take(1).ToArray();

                htype = pPayload.Skip(1).Take(1).ToArray();

                hlen = pPayload.Skip(2).Take(1).ToArray();

                hops = pPayload.Skip(3).Take(1).ToArray();

                xid = pPayload.Skip(4).Take(4).ToArray();

                secs = pPayload.Skip(8).Take(2).ToArray();

                flags = pPayload.Skip(10).Take(2).ToArray();

                ciaddr = pPayload.Skip(12).Take(4).ToArray();

                yiaddr = pPayload.Skip(16).Take(4).ToArray();

                siaddr = pPayload.Skip(20).Take(4).ToArray();

                giaddr = pPayload.Skip(24).Take(4).ToArray();

                chaddr = pPayload.Skip(28).Take(6).ToArray();

                chaddrPadding = pPayload.Skip(34).Take(10).ToArray();

                sname = pPayload.Skip(44).Take(64).ToArray();

                file = pPayload.Skip(108).Take(128).ToArray();

                magicCookie = pPayload.Skip(236).Take(4).ToArray();

                dhcpOptions = pPayload.Skip(240).ToArray();

                return true;
            }
            catch (Exception) { }

            return false;
        }
    }  

    /// <summary>
    ///  Create a DHCP option, as listed in RFC 2132[13] and IANA registry with optionId-Enum 
    /// </summary>
    public class DhcpOption
    {
        /// <summary>
        /// Define the DHCP options to be created by name
        /// </summary>
        public dhcpOptionIds optionId { get; set; } = new dhcpOptionIds();
        private byte[] optionIdBytes = new byte[] { };

        /// <summary>
        /// Define the required length for the optionValue
        /// </summary>
        public byte[] optionLength { get; set; } = new byte[] { };

        /// <summary>
        /// Define the value for the option e.g. subnet mask
        /// </summary>
        public byte[] optionValue { get; set; } = new byte[] { };

        /// <summary>
        /// Create the DHCP option as byte array. Is then specified as an option in the DhcpPacket.
        /// </summary>
        /// <returns></returns>
        public byte[] buildDhcpOption()
        {
            if (Enum.IsDefined(typeof(dhcpOptionIds), optionId))
            {
                object selected = Convert.ChangeType(optionId, optionId.GetTypeCode());
                optionIdBytes = new byte[] { Convert.ToByte(selected, null) };
            }           

            return optionIdBytes.Concat(optionLength).Concat(optionValue).ToArray();
        }

        public List<DhcpOption> parseDhcpOptions(byte[] pPayload)
        {
            List<DhcpOption> dhcpOptionList = new List<DhcpOption>();

            using (MemoryStream memoryStream = new MemoryStream(pPayload))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    bool endfound = false;

                    while (!endfound)
                    {
                        byte[] dhcpOptionID = binaryReader.ReadBytes(1);
                        byte[] dhcpOptionValueLength = new byte[] { };
                        byte[] dhcpOptionValue = null;

                        if (Convert.ToInt32(dhcpOptionID) != 255)
                        {
                            dhcpOptionValueLength = binaryReader.ReadBytes(1);
                            dhcpOptionValue = binaryReader.ReadBytes(Convert.ToInt32(dhcpOptionValueLength));
                        }
                        else
                        {
                            endfound = true;
                        }

                        DhcpOption dhcpOption = new DhcpOption
                        {
                            optionIdBytes = dhcpOptionID,
                            optionLength = dhcpOptionValueLength,
                            optionValue = dhcpOptionValue,
                        };

                        dhcpOptionList.Add(dhcpOption);
                    }                    
                }
            }

            return dhcpOptionList;
        }

    }

    public enum dhcpOptionIds : ushort
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

        // BOOTP Vendor Information Extensions
        End = 255,
    }
}
