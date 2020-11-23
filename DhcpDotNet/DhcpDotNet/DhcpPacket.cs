using System;
using System.Collections.Generic;
using System.Linq;

namespace DhcpDotNet
{
    /// <summary>
    /// Creates an empty predefined DHCP packet in the form of a byte array.
    /// </summary>
    public class DhcpPacket
    {
        /// <summary>
        /// Information whether it is a request (request = 1) or a reply (reply = 2)
        /// </summary>
        public byte[] messageType { get; set; } = new byte[1] { 0x01 };

        /// <summary>
        /// Network type (e.g. 1 = Ethernet, 6 = IEEE 802 networks or 7 = ARCNET)
        /// </summary>
        public byte[] hardwareType { get; set; } = new byte[1] { 0x01 };

        /// <summary>
        /// Length of the physical network address in bytes (e.g. 6 = MAC/Ethernet address)
        /// </summary>
        public byte[] hardwareAddressLength { get; set; } = new byte[1] { 0x06 };

        /// <summary>
        /// Number of DHCP relay agents on the data path
        /// </summary>
        public byte[] hops { get; set; } = new byte[1] { 0x00 };

        /// <summary>
        ///  ID of the connection between client and server
        /// </summary>
        public byte[] transactionID { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Time in seconds since the client was started. Default is 12secs
        /// </summary>
        public byte[] secs { get; set; } = new byte[2] { 0x0c, 0x00 };

        /// <summary>
        /// Currently, only the first bit is used (indicates if the client still has a valid IP address), the remaining bits are reserved for later protocol extensions
        /// </summary>
        public byte[] bootpFlags { get; set; } = new byte[2] { 0x00, 0x00 };

        /// <summary>
        /// Client-IP-Adresse
        /// </summary>
        public byte[] clientIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// own IP-address
        /// </summary>
        public byte[] yourIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Server-IP-Adresse
        /// </summary>
        public byte[] nextServerIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Relay-Agent-IP-Adresse
        /// </summary>
        public byte[] relayAgentIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Client-MAC-Adresse
        /// </summary>
        public byte[] clientMac { get; set; } = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// clientMacPadding
        /// </summary>
        public byte[] clientMacPadding { get; set; } = new byte[10] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Name of the DHCP server, if a specific one is required (contains C-string), specification optional
        /// </summary>
        public byte[] serverHostname { get; set; } = new byte[64] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Name of a file (e.g. system kernel) to be sent from the server to the client via TFTP (contains C-string), specification optional
        /// </summary>
        public byte[] bootFilename { get; set; } = new byte[128] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

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
            return messageType.Concat(hardwareType).Concat(hardwareAddressLength).Concat(hops).Concat(transactionID).Concat(secs).Concat(bootpFlags).Concat(clientIP).Concat(yourIP).Concat(nextServerIP).Concat(relayAgentIP).Concat(clientMac).Concat(clientMacPadding).Concat(serverHostname).Concat(bootFilename).Concat(magicCookie).Concat(dhcpOptions).Concat(end).ToArray();
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
                messageType = pPayload.Take(1).ToArray();

                hardwareType = pPayload.Skip(1).Take(1).ToArray();

                hardwareAddressLength = pPayload.Skip(2).Take(1).ToArray();

                hops = pPayload.Skip(3).Take(1).ToArray();

                transactionID = pPayload.Skip(4).Take(4).ToArray();

                secs = pPayload.Skip(8).Take(2).ToArray();

                bootpFlags = pPayload.Skip(10).Take(2).ToArray();

                clientIP = pPayload.Skip(12).Take(4).ToArray();

                yourIP = pPayload.Skip(16).Take(4).ToArray();

                nextServerIP = pPayload.Skip(20).Take(4).ToArray();

                relayAgentIP = pPayload.Skip(24).Take(4).ToArray();

                clientMac = pPayload.Skip(28).Take(6).ToArray();

                clientMacPadding = pPayload.Skip(34).Take(10).ToArray();

                serverHostname = pPayload.Skip(44).Take(64).ToArray();

                bootFilename = pPayload.Skip(108).Take(128).ToArray();

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
            object selected = Convert.ChangeType(optionId, optionId.GetTypeCode());
            optionIdBytes = new byte[] { Convert.ToByte(selected, null) };

            return optionIdBytes.Concat(optionLength).Concat(optionValue).ToArray();
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
