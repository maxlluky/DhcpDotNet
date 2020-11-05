using System.Linq;

namespace DhcpDotNet
{
    /// <summary>
    /// Creates an empty predefined DHCP packet in the form of a byte array.
    /// </summary>
    public class DhcpPacket
    {
        /// <summary>
        /// Has MessageType, HardwareType, Hardware-Address-Length
        /// </summary>
        public byte[] firstPart { get; set; } = new byte[3] { 0x01, 0x01, 0x06 };

        /// <summary>
        /// Number of DHCP relay agents on the data path
        /// </summary>
        public byte[] hops { get; set; } = new byte[1] { 0x00 };

        /// <summary>
        ///  ID of the connection between client and server
        /// </summary>
        public byte[] transactionID { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Time in seconds since the client was started
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
        /// Defines the end of the DHCP payload
        /// </summary>
        public byte[] end { get; set; } = new byte[1] { 0xff };

        /// <summary>
        /// Creates a byte array in the form of a DHCP payload, which can be sent via a UDP datagram. 
        /// </summary>
        /// /// <returns></returns>
        public byte[] buildPacket()
        {
            return firstPart.Concat(hops).Concat(transactionID).Concat(secs).Concat(bootpFlags).Concat(clientIP).Concat(yourIP).Concat(nextServerIP).Concat(relayAgentIP).Concat(clientMac).Concat(clientMacPadding).Concat(serverHostname).Concat(bootFilename).Concat(magicCookie).Concat(dhcpOptions).Concat(end).ToArray();
        }
    }

    /// <summary>
    ///  Create a DHCP option, as listed in RFC 2132[13] and IANA registry.
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
            switch (optionId)
            {
                case (dhcpOptionIds)1:
                    optionIdBytes = new byte[] { 0x35 };
                    break;
                case (dhcpOptionIds)2:
                    optionIdBytes = new byte[] { 0x3d };
                    break;
                case (dhcpOptionIds)3:
                    optionIdBytes = new byte[] { 0x32 };
                    break;
                case (dhcpOptionIds)4:
                    optionIdBytes = new byte[] { 0x36 };
                    break;
                case (dhcpOptionIds)5:
                    optionIdBytes = new byte[] { 0x33 };
                    break;
                case (dhcpOptionIds)6:
                    optionIdBytes = new byte[] { 0x3a };
                    break;
                case (dhcpOptionIds)7:
                    optionIdBytes = new byte[] { 0x3b };
                    break;
                case (dhcpOptionIds)8:
                    optionIdBytes = new byte[] { 0x31 };
                    break;
                case (dhcpOptionIds)9:
                    optionIdBytes = new byte[] { 0x03 };
                    break;
                case (dhcpOptionIds)10:
                    optionIdBytes = new byte[] { 0x06 };
                    break;
                case (dhcpOptionIds)11:
                    optionIdBytes = new byte[] { 0x0f };
                    break;
                case (dhcpOptionIds)12:
                    optionIdBytes = new byte[] { 0x0c };
                    break;
                case (dhcpOptionIds)13:
                    optionIdBytes = new byte[] { 0x51 };
                    break;
                case (dhcpOptionIds)14:
                    optionIdBytes = new byte[] { 0x3c };
                    break;
                case (dhcpOptionIds)15:
                    optionIdBytes = new byte[] { 0x37 };
                    break;
                case (dhcpOptionIds)16:
                    optionIdBytes = new byte[] { 0x1c };
                    break;
                case (dhcpOptionIds)17:
                    optionIdBytes = new byte[] { 0x2a };
                    break;
                case (dhcpOptionIds)18:
                    optionIdBytes = new byte[] { 0x9e };
                    break;
            }

            return optionIdBytes.Concat(optionLength).Concat(optionValue).ToArray();
        }
    }

    public enum dhcpOptionIds : ushort
    {
        DhcpMessageType = 1,
        ClientIdentifier = 2,
        RequestedIpAddress = 3,
        DhcpServerIdentifier = 4,
        IpAddressLeaseTime = 5,
        RenewalTimeValue = 6,
        RebindingTimeValue = 7,
        SubnetMask = 8,
        Router = 9,
        DomainNameServer = 10,
        DomainName = 11,
        Hostname = 12,
        ClientFullyQualifiedDomainName = 13,
        VendorClassIdentifier = 14,
        ParameterRequestList = 15,
        BroadcastAddress = 16,
        NetworkTimeProtocolServers = 17,
        PCPServer = 18,
    }
}
