using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DhcpDotNet
{
    /// <summary>
    /// Represents a DHCPv6 message and provides methods to build it into a byte array or to parse a
    /// received UDP payload back into its fields. See RFC 8415 for detailed information:
    /// https://tools.ietf.org/html/rfc8415
    /// </summary>
    public class Dhcpv6Packet
    {
        /// <summary>
        /// Identifies the DHCPv6 message type; see <see cref="Dhcpv6MessageType"/> and section 7.3 of
        /// RFC 8415. A 1-octet field.
        /// </summary>
        public byte msgtype { get; set; } = (byte)Dhcpv6MessageType.ADVERTISE;

        /// <summary>
        /// The transaction ID for this message exchange. A 3-octet field.
        /// </summary>
        public byte[] transactionid { get; set; } = new byte[3];

        /// <summary>
        /// Options carried in this message. Build them with <see cref="Dhcpv6Option"/>. A variable-length field.
        /// </summary>
        public byte[] options { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Creates a byte array in the form of a DHCPv6 payload, which can be sent via a UDP datagram.
        /// </summary>
        public byte[] buildPacket()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(msgtype);
                binaryWriter.Write(transactionid);
                binaryWriter.Write(options);
                binaryWriter.Flush();

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Parses a raw DHCPv6 payload. The return value indicates whether the process was successful.
        /// </summary>
        /// <param name="pPayload">The entire UDP payload.</param>
        public bool parsePacket(byte[] pPayload)
        {
            if (pPayload == null || pPayload.Length < 4)
            {
                return false;
            }

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(pPayload))
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    msgtype = binaryReader.ReadByte();
                    transactionid = binaryReader.ReadBytes(3);
                    options = binaryReader.ReadBytes(pPayload.Length - 4);
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
        /// Parses and returns the list of DHCPv6 options contained in this packet's <see cref="options"/>.
        /// </summary>
        public List<Dhcpv6Option> getOptions() => new Dhcpv6Option().parseDhcpOptions(options);
    }

    /// <summary>
    /// Create a DHCPv6 option, as listed in RFC 8415 and the IANA registry via the <see cref="Dhcpv6OptionIds"/> enum.
    /// </summary>
    public class Dhcpv6Option
    {
        /// <summary>
        /// The DHCPv6 option to be created, by name.
        /// </summary>
        public Dhcpv6OptionIds optionId { get; set; }

        /// <summary>
        /// The 2-octet option code in network byte order. Set automatically from <see cref="optionId"/>
        /// when building; also populated when parsing.
        /// </summary>
        public byte[] optionIdBytes { get; set; } = new byte[2];

        /// <summary>
        /// The 2-octet option length in network byte order. Set automatically from the value when building.
        /// </summary>
        public byte[] optionLength { get; set; } = new byte[2];

        /// <summary>
        /// The option value.
        /// </summary>
        public byte[] optionValue { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Creates the DHCPv6 option as a byte array to be appended to a packet's options section.
        /// DHCPv6 encodes option-code and option-len as 2 octets each, in network (big-endian) byte order.
        /// </summary>
        public byte[] buildDhcpOption()
        {
            if (Enum.IsDefined(typeof(Dhcpv6OptionIds), optionId))
            {
                optionIdBytes = GetBigEndianUInt16((ushort)optionId);
            }

            optionLength = GetBigEndianUInt16((ushort)optionValue.Length);

            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                binaryWriter.Write(optionIdBytes);
                binaryWriter.Write(optionLength);
                binaryWriter.Write(optionValue);
                binaryWriter.Flush();

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Parses the raw options section of a DHCPv6 packet into a list of <see cref="Dhcpv6Option"/>.
        /// </summary>
        public List<Dhcpv6Option> parseDhcpOptions(byte[] pPayload)
        {
            List<Dhcpv6Option> dhcpOptionList = new List<Dhcpv6Option>();

            if (pPayload == null || pPayload.Length < 4)
            {
                return dhcpOptionList;
            }

            int index = 0;
            while (index + 4 <= pPayload.Length)
            {
                byte[] code = new byte[] { pPayload[index], pPayload[index + 1] };
                byte[] lengthBytes = new byte[] { pPayload[index + 2], pPayload[index + 3] };
                int length = (lengthBytes[0] << 8) | lengthBytes[1];
                index += 4;

                if (index + length > pPayload.Length)
                {
                    // Truncated option; stop rather than reading past the buffer.
                    break;
                }

                byte[] value = new byte[length];
                Array.Copy(pPayload, index, value, 0, length);
                index += length;

                ushort codeValue = (ushort)((code[0] << 8) | code[1]);

                dhcpOptionList.Add(new Dhcpv6Option
                {
                    optionIdBytes = code,
                    optionId = Enum.IsDefined(typeof(Dhcpv6OptionIds), codeValue)
                        ? (Dhcpv6OptionIds)codeValue
                        : default,
                    optionLength = lengthBytes,
                    optionValue = value,
                });
            }

            return dhcpOptionList;
        }

        private static byte[] GetBigEndianUInt16(ushort value)
        {
            return new byte[] { (byte)(value >> 8), (byte)(value & 0xff) };
        }
    }

    /// <summary>
    /// DHCPv6 message types. See section 7.3 of RFC 8415.
    /// </summary>
    public enum Dhcpv6MessageType : byte
    {
        SOLICIT = 1,
        ADVERTISE = 2,
        REQUEST = 3,
        CONFIRM = 4,
        RENEW = 5,
        REBIND = 6,
        REPLY = 7,
        RELEASE = 8,
        DECLINE = 9,
        RECONFIGURE = 10,
        INFORMATION_REQUEST = 11,
        RELAY_FORW = 12,
        RELAY_REPL = 13,
    }

    /// <summary>
    /// DHCPv6 option ids as listed in RFC 8415 and the IANA registry.
    /// </summary>
    public enum Dhcpv6OptionIds : ushort
    {
        CLIENTID = 1,
        SERVERID = 2,
        IA_NA = 3,
        IA_TA = 4,
        IAADDR = 5,
        ORO = 6,
        PREFERENCE = 7,
        ELAPSED_TIME = 8,
        RELAY_MSG = 9,
        AUTH = 11,
        UNICAST = 12,
        STATUS_CODE = 13,
        RAPID_COMMIT = 14,
        USER_CLASS = 15,
        VENDOR_CLASS = 16,
        VENDOR_OPTS = 17,
        INTERFACE_ID = 18,
        RECONF_MSG = 19,
        RECONF_ACCEPT = 20,
        SIP_SERVER_D = 21,
        SIP_SERVER_A = 22,
        DNS_SERVERS = 23,
        DOMAIN_LIST = 24,
        IA_PD = 25,
        IAPREFIX = 26,
        NIS_SERVERS = 27,
        NISP_SERVERS = 28,
        NIS_DOMAIN_NAME = 29,
        NISP_DOMAIN_NAME = 30,
        SNTP_SERVERS = 31,
        INFORMATION_REFRESH_TIME = 32,
        BCMCS_SERVER_D = 33,
        BCMCS_SERVER_A = 34,
        GEOCONF_CIVIC = 36,
        REMOTE_ID = 37,
        SUBSCRIBER_ID = 38,
        CLIENT_FQDN = 39,
        PANA_AGENT = 40,
        NEW_POSIX_TIMEZONE = 41,
        NEW_TZDB_TIMEZONE = 42,
        ERO = 43,
        LQ_QUERY = 44,
        CLIENT_DATA = 45,
        CLT_TIME = 46,
        LQ_RELAY_DATA = 47,
        LQ_CLIENT_LINK = 48,
        MIP6_HNIDF = 49,
        MIP6_VDINF = 50,
        V6_LOST = 51,
        CAPWAP_AC_V6 = 52,
        RELAY_ID = 53,
        IPv6_AddressMoS = 54,
        IPv6_FQDNMoS = 55,
        NTP_SERVER = 56,
        V6_ACCESS_DOMAIN = 57,
        SIP_UA_CS_LIST = 58,
        OPT_BOOTFILE_URL = 59,
        OPT_BOOTFILE_PARAM = 60,
        CLIENT_ARCH_TYPE = 61,
        NII = 62,
        GEOLOCATION = 63,
        AFTR_NAME = 64,
        ERP_LOCAL_DOMAIN_NAME = 65,
        RSOO = 66,
        PD_EXCLUDE = 67,
        VSS = 68,
        MIP6_IDINF = 69,
        MIP6_UDINF = 70,
        MIP6_HNP = 71,
        MIP6_HAA = 72,
        MIP6_HAF = 73,
        RDNSS_SELECTION = 74,
        KRB_PRINCIPAL_NAME = 75,
        KRB_REALM_NAME = 76,
        KRB_DEFAULT_REALM_NAME = 77,
        KRB_KDC = 78,
        CLIENT_LINKLAYER_ADDR = 79,
        LINK_ADDRESS = 80,
        RADIUS = 81,
        SOL_MAX_RT = 82,
        INF_MAX_RT = 83,
        ADDRSEL = 84,
        ADDRSEL_TABLE = 85,
        V6_PCP_SERVER = 86,
        DHCPV4_MSG = 87,
        DHCP4_O_DHCP6_SERVER = 88,
        S46_RULE = 89,
        S46_BR = 90,
        S46_DMR = 91,
        S46_V4V6BIND = 92,
        S46_PORTPARAMS = 93,
        S46_CONT_MAPE = 94,
        S46_CONT_MAPT = 95,
        S46_CONT_LW = 96,
        _4RD = 97,
        _4RD_MAP_RULE = 98,
        _4RD_NON_MAP_RULE = 99,
        LQ_BASE_TIME = 100,
        LQ_START_TIME = 101,
        LQ_END_TIME = 102,
        DHCPCaptivePortal = 103,
        MPL_PARAMETERS = 104,
        ANI_ATT = 105,
        ANI_NETWORK_NAME = 106,
        ANI_AP_NAME = 107,
        ANI_AP_BSSID = 108,
        ANI_OPERATOR_ID = 109,
        ANI_OPERATOR_REALM = 110,
        S46_PRIORITY = 111,
        MUD_URL_V6 = 112,
        V6_PREFIX64 = 113,
        F_BINDING_STATUS = 114,
        F_CONNECT_FLAGS = 115,
        F_DNS_REMOVAL_INFO = 116,
        F_DNS_HOST_NAME = 117,
        F_DNS_ZONE_NAME = 118,
        F_DNS_FLAGS = 119,
        F_EXPIRATION_TIME = 120,
        F_MAX_UNACKED_BNDUPD = 121,
        F_MCLT = 122,
        F_PARTNER_LIFETIME = 123,
        F_PARTNER_LIFETIME_SENT = 124,
        F_PARTNER_DOWN_TIME = 125,
        F_PARTNER_RAW_CLT_TIME = 126,
        F_PROTOCOL_VERSION = 127,
        F_KEEPALIVE_TIME = 128,
        F_RECONFIGURE_DATA = 129,
        F_RELATIONSHIP_NAME = 130,
        F_SERVER_FLAGS = 131,
        F_SERVER_STATE = 132,
        F_START_TIME_OF_STATE = 133,
        F_STATE_EXPIRATION_TIME = 134,
        RELAY_PORT = 135,
        IPv6_AddressANDSF = 143,
    }
}
