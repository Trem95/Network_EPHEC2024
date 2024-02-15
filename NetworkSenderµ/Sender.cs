using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkSender
{

    class Sender
    {
        static UdpClient udpClient;
        static IPEndPoint remoteEndPoint;
        static FileStream fileStream;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: Sender <receiverIp> <receiverPort> <inputFile>");
                return;
            }

            string receiverIp = args[0];
            int receiverPort = int.Parse(args[1]);
            string inputFile = args[2];

            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(receiverIp), receiverPort);
            fileStream = File.OpenRead(inputFile);

            // Implement connection initiation logic (SYN) and data transmission
            SendSynPacket();
            
            //TODO add behavior for sending packet
            //- check if all packet come to right receiver
            //- manage sequence number
            //- manage re send of packet if loss

            // Implement connection termination logic (FIN, RST)
            SendFinPacket();
        }

        static void SendSynPacket()
        {
            byte[] synPacket = ConstructPacket(0, 1, 0, 0, new byte[0]);

            udpClient.Send(synPacket, synPacket.Length, remoteEndPoint);

            // Wait for SYN-ACK
            byte[] synAckData = udpClient.Receive(ref remoteEndPoint);
            ushort synAckSequenceNumber = GetLastSequenceNumber(synAckData);

            // Respond with final SYN
            byte[] finalSynPacket = ConstructPacket(synAckSequenceNumber, 1, 0, 0, new byte[0]);
            udpClient.Send(finalSynPacket, finalSynPacket.Length, remoteEndPoint);
        }

        static void SendDataPacket(byte[] data, ushort sequenceNumber)
        {
            byte[] dataPacket = ConstructPacket(sequenceNumber, 0, 0, 0, data);
            udpClient.Send(dataPacket, dataPacket.Length, remoteEndPoint);
        }

        static void SendFinPacket()
        {
            //TODO
        }

        static void SendRstPacket()
        {
            //TODO
        }

        static ushort GetLastSequenceNumber(byte[] data)
        {
            // last two bytes is seq numb (see if ok ) 
            return BitConverter.ToUInt16(data, data.Length - 2);
        }

        static byte[] ConstructPacket(ushort sequenceNumber, byte synFlag, byte ackFlag, byte finFlag, byte[] data)
        {
            // Implement logic to construct a packet with given parameters
            byte[] packet = new byte[ 5 + data.Length];

            // Copy sequence number
            BitConverter.GetBytes(sequenceNumber).CopyTo(packet, 0);

            // Set flags
            packet[2] = synFlag;
            packet[3] = ackFlag;
            packet[4] = finFlag;

            // Copy data
            data.CopyTo(packet, 5);

            return packet;
        }
    }

}