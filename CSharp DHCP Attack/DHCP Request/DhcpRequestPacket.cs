using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class DhcpRequestPacket
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

    //--GROUP OPTION DHCP Message Type      --------------------------------------------------------------------------
    private byte[] optDhcpMessageType = new byte[1] { 0x35 };
    private byte[] dhcpMessageTypeLength = new byte[1] { 0x01 };
    //--dhcpMessageType (Discover)
    public byte[] dhcpMessageType { get; set; } = new byte[1] { 0x00 };
    //--END GROUP DHCP Message Type         --------------------------------------------------------------------------

    //--GROUP OPTION Client Identifier      --------------------------------------------------------------------------
    //--clientIdentifier
    private byte[] optClientId = new byte[1] { 0x3d };
    private byte[] clientIdLength = new byte[1] { 0x07 };
    private byte[] clientIdHwType = new byte[1] { 0x01 };

    //--clientIdentifier : Client MAC-Address in Option
    public byte[] clientIdentifier { get; set; } = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    //--END GROUP Client Identifier         --------------------------------------------------------------------------

    //--requestedIP
    private byte[] requestedIPpre = new byte[2] { 0x32, 0x04 };

    //--requestedIP (change this to dest-IP)
    public byte[] requestedIP { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };

    //--GROUP OPTION dhcpServerIdentifier   --------------------------------------------------------------------------
    private byte[] optDhcpServerId = new byte[1] { 0x36 };
    private byte[] dhcpServerIdLength = new byte[1] { 0x04 };
    //--dhcpServerIdentifier
    public byte[] dhcpServerId { get; set; } = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
    //--END GROUP dhcpServerIdentifier      --------------------------------------------------------------------------

    //--GROUP Hostname                      --------------------------------------------------------------------------
    private byte[] optHostName = new byte[1] { 0x0C };
    public byte[] hostNameLength { get; set; } = new byte[1] { 0x06 };

    //--hostName : hostname of the client
    public byte[] hostName { get; set; } = new byte[6] { 0x43, 0x73, 0x68, 0x61, 0x72, 0x70 };
    //--END GROUP Hostname                  --------------------------------------------------------------------------

    //--clientFQDN
    public byte[] clientFQDN = new byte[11] { 0x51, 0x09, 0x00, 0x00, 0x00, 0x70, 0x63, 0x5f, 0x6d, 0x61, 0x78 };

    //--vendorClassIndentifier
    public byte[] vendorClassIndentifier { get; set; } = new byte[10] { 0x3c, 0x08, 0x4d, 0x53, 0x46, 0x54, 0x20, 0x35, 0x2e, 0x30 };

    //--parameterRequestList
    public byte[] parameterRequestList { get; set; } = new byte[16] { 0x37, 0x0e, 0x01, 0x03, 0x06, 0x0f, 0x1f, 0x21, 0x2b, 0x2c, 0x2e, 0x2f, 0x77, 0x79, 0xf9, 0xfc };
    
    //--end
    public byte[] end { get; set; } = new byte[1] { 0xff };

    public byte[] buildPacket()
    {
        return firstPart.Concat(hops).Concat(transactionID).Concat(secs).Concat(bootpFlags).Concat(clientIP).Concat(yourIP).Concat(nextServerIP).Concat(relayAgentIP).Concat(clientMac).Concat(clientMacPadding).Concat(serverHostname).Concat(bootFilename).Concat(magicCookie).Concat(optDhcpMessageType).Concat(dhcpMessageTypeLength).Concat(dhcpMessageType).Concat(optClientId).Concat(clientIdLength).Concat(clientIdHwType).Concat(clientIdentifier).Concat(requestedIPpre).Concat(requestedIP).Concat(optDhcpServerId).Concat(dhcpServerIdLength).Concat(dhcpServerId).Concat(optHostName).Concat(hostNameLength).Concat(hostName).Concat(clientFQDN).Concat(vendorClassIndentifier).Concat(parameterRequestList).Concat(end).ToArray();
    }
}
