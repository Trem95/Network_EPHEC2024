using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkReceiver
{
    class Receiver
    {
        static UdpClient udpClient;
        static IPEndPoint remoteEndPoint;
        static FileStream fileStream;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Receiver <port> <outputFile>");
                return;
            }
            Console.WriteLine("TEST");
            int port = int.Parse(args[0]);
            string outputFile = args[1];

            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            fileStream = File.OpenWrite(outputFile);

            while (true)
            {
                Console.WriteLine("ON WHILE");
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                Console.WriteLine(data.ToString());
                Console.WriteLine("RECEIVED");
                ProcessReceivedData(data);
                //TODO implement behavior for the several flags

                SendAckPacket(GetLastSequenceNumber(data));
            }
        }

        static void ProcessReceivedData(byte[] data)
        {
            //TODO write data
            fileStream.Write(data, 0, data.Length);
        }

        static void ManageSenderPacket()
        {

            //TODO method to set the behavior when get a packet from sender
            switch (switch_on)
            {
                default:
            }
        }

        static void SendAckPacket(ushort lastSequenceNumber)
        {
            byte[] ackData = BitConverter.GetBytes(lastSequenceNumber);
            byte[] ackPacket = CreatePacket(0, 0, 0, 0, 0, ackData);

            udpClient.Send(ackPacket, ackPacket.Length, remoteEndPoint);
        }

        static void SendFinPacket(ushort lastSequenceNumber)
        {
            //TODO
        }

        static void SendRstPacket(ushort lastSequenceNumber)
        {
            //TODO
        }

        static void SendSynPacket(ushort lastSequenceNumber) 
        {
            //TODO
        }

        static ushort GetLastSequenceNumber(byte[] data)
        {
            //TODO check for sequence number behavior
            return BitConverter.ToUInt16(data, data.Length - 2);
        }

        static byte[] CreatePacket(ushort sequenceNumber, byte synFlag, byte ackFlag, byte finFlag, byte rstFlag, byte[]? data = null)
        {
            byte[] packet = new byte[6 + (data != null ? data.Length : 0)];

            BitConverter.GetBytes(sequenceNumber).CopyTo(packet, 0);

            packet[2] = synFlag;
            packet[3] = ackFlag;
            packet[4] = finFlag;
            packet[5] = rstFlag;

            if (data != null)
                data.CopyTo(packet, 6);

            return packet;
        }
    }
}