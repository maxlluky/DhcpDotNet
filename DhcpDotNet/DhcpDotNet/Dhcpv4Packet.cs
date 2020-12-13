using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DhcpDotNet
{
    /// <summary>
    /// Creates an empty predefined DHCPv4 packet in the form of a byte array. Please visit RFC 2131 for detaied information: https://tools.ietf.org/html/rfc2131
    /// </summary>
    public class DHCPv4Packet
    {
        /// <summary>
        /// Message op code / message type. 1 = BOOTREQUEST, 2 = BOOTREPLY
        /// </summary>
        public byte op { get; set; } = new byte();

        /// <summary>
        /// Hardware address type, see ARP section in "Assigned Numbers" RFC; e.g., '1' = 10mb ethernet.
        /// </summary>
        public byte htype { get; set; } = new byte();

        /// <summary>
        /// Hardware address length (e.g.  '6' for 10mb ethernet).
        /// </summary>
        public byte hlen { get; set; } = new byte();

        /// <summary>
        /// Client sets to zero, optionally used by relay agents when booting via a relay agent.
        /// </summary>
        public byte hops { get; set; } = new byte();

        /// <summary>
        ///  Transaction ID, a random number chosen by the client, used by the client and server to associate messages and responses between a client and a server.
        /// </summary>
        public byte[] xid { get; set; } = new byte[4];

        /// <summary>
        /// Filled in by client, seconds elapsed since client began address acquisition or renewal process.
        /// </summary>
        public byte[] secs { get; set; } = new byte[2];

        /// <summary>
        /// Flags (see figure 2).
        /// </summary>
        public byte[] flags { get; set; } = new byte[2];

        /// <summary>
        /// Client IP address; only filled in if client is in BOUND, RENEW or REBINDING state and can respond to ARP requests.
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
        /// If your chaddr is not 6 bytes long use this to match the chaddress-size of 16 bytes.
        /// </summary>
        public byte[] chaddrPadding { get; set; } = new byte[10];

        /// <summary>
        /// Optional server host name, null terminated string.
        /// </summary>
        public byte[] sname { get; set; } = new byte[64];

        /// <summary>
        /// Boot file name, null terminated string; "generic" name or null in DHCPDISCOVER, fully qualified directory-path name in DHCPOFFER.
        /// </summary>
        public byte[] file { get; set; } = new byte[128];

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
        /// Creates a byte array in the form of a DHCPv4 payload, which can be sent via a UDP datagram. 
        /// </summary>
        /// /// <returns></returns>
        public byte[] buildPacket()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
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
                }
                memoryStream.Flush();

                return memoryStream.GetBuffer();
            }
        }

        /// <summary>
        /// Parses a raw-DHCP payload. The return value indicates whether the process was successful or not. 
        /// </summary>
        /// <param name="pPayload">The entire Udp payload must be transferred</param>
        /// <returns></returns>
        public bool parsePacket(byte[] pPayload)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(pPayload))
                {
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
                        chaddr = binaryReader.ReadBytes(Convert.ToInt32(hlen));
                        chaddrPadding = binaryReader.ReadBytes(Convert.ToInt32(16 - chaddr.Length));
                        sname = binaryReader.ReadBytes(64);
                        file = binaryReader.ReadBytes(128);
                        magicCookie = binaryReader.ReadBytes(4);
                        dhcpOptions = binaryReader.ReadBytes(pPayload.Length - Convert.ToInt32(binaryReader.BaseStream.Position));
                    }
                }
                return true;
            }
            catch (Exception eX) 
            {
                Debug.WriteLine("DhcpDotNet-Exception: " + eX.Message);
            }
            return false;
        }
    }

    /// <summary>
    ///  Create a DHCP option, as listed in RFC 2132[13] and IANA registry with optionId-Enum
    /// </summary>
    public class DHCPv4Option
    {
        /// <summary>
        /// Define the DHCPv4 options to be created by name
        /// </summary>
        public DHCPv4OptionIds optionId { get; set; } = new DHCPv4OptionIds();

        /// <summary>
        /// Represents the optionId (enum) in bytes. This field is not required if you set optionId with enum.
        /// </summary>
        public byte optionIdBytes { get; set; } = new byte();

        /// <summary>
        /// Define the required length for the optionValue
        /// </summary>
        public byte optionLength { get; set; } = new byte();

        /// <summary>
        /// Define the value for the option e.g. subnet mask
        /// </summary>
        public byte[] optionValue { get; set; } = new byte[] { };

        /// <summary>
        /// Create the DHCPv4 option as byte array. Is then specified as an option in the DhcpPacket.
        /// </summary>
        /// <returns></returns>
        public byte[] buildDhcpOption()
        {
            if (Enum.IsDefined(typeof(DHCPv4OptionIds), optionId))
            {
                object selected = Convert.ChangeType(optionId, optionId.GetTypeCode());
                optionIdBytes = Convert.ToByte(selected, null);
            }

            byte[] result = new byte[] { optionIdBytes, optionLength };
            return result.Concat(optionValue).ToArray();
        }

        public List<DHCPv4Option> parseDhcpOptions(byte[] pPayload)
        {
            List<DHCPv4Option> dhcpOptionList = new List<DHCPv4Option>();

            using (MemoryStream memoryStream = new MemoryStream(pPayload))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    bool endfound = false;

                    do
                    {
                        byte dhcpOptionID = binaryReader.ReadByte();
                        byte dhcpOptionValueLength = new byte();
                        byte[] dhcpOptionValue = null;

                        if (!dhcpOptionID.Equals(0xff))
                        {
                            dhcpOptionValueLength = binaryReader.ReadByte();
                            dhcpOptionValue = binaryReader.ReadBytes(Convert.ToInt32(dhcpOptionValueLength));

                            DHCPv4Option dhcpOption = new DHCPv4Option
                            {
                                optionIdBytes = dhcpOptionID,
                                optionLength = dhcpOptionValueLength,
                                optionValue = dhcpOptionValue,
                            };

                            dhcpOptionList.Add(dhcpOption);
                        }
                        else
                        {
                            endfound = true;
                        }
                    }
                    while (!endfound);
                }
            }

            return dhcpOptionList;
        }
    }

    public enum DHCPv4OptionIds : ushort
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
