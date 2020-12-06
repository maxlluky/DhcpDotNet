using System;
using System.IO;

namespace DhcpDotNet
{
    /// <summary>
    /// Creates an empty predefined DHCPv6 packet (Advertise) in the form of a byte array. Please visit RFC 8415 for detaied information: https://tools.ietf.org/html/rfc8415
    /// </summary>
    class DHCPv6Packet
    {
        /// <summary>
        /// Identifies the DHCP message type; the available message types are listed in Section 7.3 (RFC 8145). A 1-octet field.
        /// </summary>
        public byte msgtype { get; set; } = 0x02;

        /// <summary>
        /// The transaction ID for this message exchange. A 3-octet field.
        /// </summary>
        public byte[] transactionid { get; set; } = new byte[3];

        /// <summary>
        /// Options carried in this message; options are described in Section 21 (RFC 8145). A variable-length field (4 octets less than the size of the message).
        /// </summary>
        public byte[] options { get; set; } = new byte[] { };

        /// <summary>
        /// Creates a byte array in the form of a DHCPv6 payload, which can be sent via a UDP datagram. 
        /// </summary>
        /// <returns></returns>
        public byte[] buildPacket()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(msgtype);
                    binaryWriter.Write(transactionid);
                    binaryWriter.Write(options);
                }
                memoryStream.Flush();

                return memoryStream.GetBuffer();
            }
        }

        /// <summary>
        /// Parses a raw-DHCPv6 payload. The return value indicates whether the process was successful or not. 
        /// </summary>
        /// <param name="pPayload">The entire Udp payload must be transferred</param>
        /// <returns></returns>
        public bool parsePacket(byte[] pPayload)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        msgtype = binaryReader.ReadByte();
                        transactionid = binaryReader.ReadBytes(3);
                        options = binaryReader.ReadBytes(int.MaxValue);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Create a DHCPv6 option, as listed in RFC 8415[24] and IANA registry with optionId-Enum 
    /// </summary>
    class DHCPv6Option
    {
        /// <summary>
        /// Define the DHCPv6 options to be created by name
        /// </summary>
        public DHCPv6OptionIds optionId { get; set; } = new DHCPv6OptionIds();

        /// <summary>
        /// Represents the optionId (enum) in bytes. This field is not required if you set optionId with enum.
        /// </summary>
        public byte[] optionIdBytes { get; set; } = new byte[2];

        /// <summary>
        /// Define the required length for the optionValue
        /// </summary>
        public byte[] optionLength { get; set; } = new byte[2];

        /// <summary>
        /// Define the value for the option e.g. subnet mask
        /// </summary>
        public byte[] optionValue { get; set; } = new byte[] { };

        /// <summary>
        /// Create the DHCPv6 option as byte array. Is then specified as an option in the DhcpPacket.
        /// </summary>
        /// <returns></returns>
        public byte[] buildDhcpOption()
        {
            if (Enum.IsDefined(typeof(DHCPv6OptionIds), optionId))
            {
                optionIdBytes = BitConverter.GetBytes((int)optionId);
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(optionIdBytes);
                    binaryWriter.Write(optionLength);
                    binaryWriter.Write(optionValue);
                }
                memoryStream.Flush();

                return memoryStream.GetBuffer();
            }
        }
    }

    public enum DHCPv6OptionIds : ushort
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
