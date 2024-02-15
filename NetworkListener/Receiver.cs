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

            int port = int.Parse(args[0]);
            string outputFile = args[1];

            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            fileStream = File.OpenWrite(outputFile);

            while (true)
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                ProcessReceivedData(data);

                // Implement connection termination logic based on received flags (RST, FIN)
                // ...

                // Send ACK packet with the sequence number of the last received packet
                SendAckPacket(GetLastSequenceNumber(data));
            }
        }

        static void ProcessReceivedData(byte[] data)
        {
            // Implement logic to process received data and write to the file stream
            // For simplicity, just write the received data to the file
            fileStream.Write(data, 0, data.Length);
        }

        static void SendAckPacket(ushort lastSequenceNumber)
        {
            byte[] ackData = BitConverter.GetBytes(lastSequenceNumber);
            byte[] ackPacket = ConstructPacket(0, 0, 0, 0, ackData);

            udpClient.Send(ackPacket, ackPacket.Length, remoteEndPoint);
        }

        static ushort GetLastSequenceNumber(byte[] data)
        {
            //For now, seq numb juste be last to
            return BitConverter.ToUInt16(data, data.Length - 2);
        }

        static byte[] ConstructPacket(ushort sequenceNumber, byte synFlag, byte ackFlag, byte finFlag, byte[] data)
        {
            // Implement logic to construct a packet with given parameters
            byte[] packet = new byte[2 + 2 + 1 + 1 + 1 + data.Length];

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