using System;
using System.Net;
using System.Runtime.InteropServices;

class Interface
{
    /// <summary>
    /// sends an ARP Request to an IP-Address and returns a HW-Address of the destination.
    /// </summary>
    /// <param name="pIPAddress">IPv4-Address of the destination-Device</param>
    /// <returns></returns>
    public string sendArpRequest(IPAddress pIPAddress)
    {
        string hwAddress = null;

        try
        {
            IPAddress hostIPAddress = pIPAddress;
            byte[] ab = new byte[6];
            int len = ab.Length,
                r = SendARP((int)hostIPAddress.Address, 0, ab, ref len);
            string tempHwAddress = BitConverter.ToString(ab, 0, 6);
            if (tempHwAddress != "00-00-00-00-00-00")
                hwAddress = tempHwAddress;
        }
        catch (Exception) { }

        return hwAddress;
    }

    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);
}