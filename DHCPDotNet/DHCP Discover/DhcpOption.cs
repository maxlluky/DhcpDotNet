using System.Linq;

class DhcpOption
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