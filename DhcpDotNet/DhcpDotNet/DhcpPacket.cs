using System.Linq;

namespace DhcpDotNet
{
    public class DhcpPacket
    {
        //-- MessageType, HardwareType, Hardware-Address-Length
        public byte[] firstPart { get; set; } = new byte[3] { 0x01, 0x01, 0x06 };

        //--hops : Anzahl der DHCP-Relay-Agents auf dem Datenpfad
        public byte[] hops { get; set; } = new byte[1] { 0x00 };

        //--transactionID : ID der Verbindung zwischen Client und Server
        public byte[] transactionID { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        //--secs : Zeit in Sekunden seit dem Start des Clients
        public byte[] secs { get; set; } = new byte[2] { 0x0c, 0x00 };

        //--BootpFlags : Z. Zt. wird nur das erste Bit verwendet (zeigt an, ob der Client noch eine gültige IP-Adresse hat), die restlichen Bits sind für spätere Protokollerweiterungen reserviert
        public byte[] bootpFlags { get; set; } = new byte[2] { 0x00, 0x00 };

        //--clientIP : Client-IP-Adresse
        public byte[] clientIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        //--yourIP : eigene IP-Adresse
        public byte[] yourIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        //--nextServerIP : Server-IP-Adresse
        public byte[] nextServerIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        //--relayAgentIP : Relay-Agent-IP-Adresse
        public byte[] relayAgentIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

        //--clientMac : Client-MAC-Adresse
        public byte[] clientMac { get; set; } = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //--clientMacPadding : 
        public byte[] clientMacPadding { get; set; } = new byte[10] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //--serverHostname :  Name des DHCP-Servers, falls ein bestimmter gefordert wird (enthält C-String), Angabe optional
        public byte[] serverHostname { get; set; } = new byte[64] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //--bootFilename :  Name einer Datei (z. B. System-Kernel), die vom Server per TFTP an den Client gesendet werden soll (enthält C-String), Angabe optional
        public byte[] bootFilename { get; set; } = new byte[128] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //--magicCookie : 
        public byte[] magicCookie { get; set; } = new byte[4] { 0x63, 0x82, 0x53, 0x63 };

        //--options : Add several DHCP options to this field
        public byte[] dhcpOptions { get; set; } = new byte[] { };

        //--end
        public byte[] end { get; set; } = new byte[1] { 0xff };


        public byte[] buildPacket()
        {
            return firstPart.Concat(hops).Concat(transactionID).Concat(secs).Concat(bootpFlags).Concat(clientIP).Concat(yourIP).Concat(nextServerIP).Concat(relayAgentIP).Concat(clientMac).Concat(clientMacPadding).Concat(serverHostname).Concat(bootFilename).Concat(magicCookie).Concat(dhcpOptions).Concat(end).ToArray();
        }
    }

    public class DhcpOption
    {
        public dhcpOptionIds optionId { get; set; } = new dhcpOptionIds();
        private byte[] optionIdBytes = new byte[] { };
        public byte[] optionLength { get; set; } = new byte[] { };
        public byte[] optionValue { get; set; } = new byte[] { };

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
    }
}
