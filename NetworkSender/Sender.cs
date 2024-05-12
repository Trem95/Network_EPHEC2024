using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetworkToolBox;

namespace NetworkSender
{

    class Sender
    {
        static UdpClient udpClient;
        static IPEndPoint remoteEndPoint;
        static FileStream fileStream;
        static ushort currentSequenceNumber;
        static Timer timer;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: Sender <receiverIp> <receiverPort> <inputFile>");
                Console.ReadLine();
                return;
            }

            string receiverIp = args[0];
            int receiverPort = int.Parse(args[1]);
            string inputFile = args[2];

            currentSequenceNumber = (ushort)new Random().Next(42);
            udpClient = new UdpClient();
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(receiverIp), receiverPort);
            fileStream = File.OpenRead(inputFile);

            // Implement connection initiation logic (SYN) and data transmission
            byte[] buffer = new byte[1024]; // TODO check for length
            int bytesRead;

            //OPEN CONNECTION
            udpClient.SendSynPacket(currentSequenceNumber, remoteEndPoint);

            currentSequenceNumber++;
            // Wait for SYN-ACK

            int cdwStartConnection = 0;
            while (cdwStartConnection < 3)
            {
                try
                {

                    try
                    {
                        timer = new Timer(callback: ToolBox.OutOfTimeCallBack, null, 5000, 0);
                        byte[] synAckData = udpClient.Receive(ref remoteEndPoint);
                        ushort synAckSequenceNumber = GetLastSequenceNumber(synAckData);//TODO Check for sequence number
                        cdwStartConnection = 3;
                    }
                    catch (TimeoutException)
                    {
                        udpClient.Close();
                    }
                }
                catch (SocketException)
                {
                    cdwStartConnection++;
                }
            }

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {

                byte[] dataToSend = new byte[bytesRead];
                Array.Copy(buffer, dataToSend, bytesRead);

                byte[] dataPacket = CreateDataPacket(dataToSend);
                udpClient.Send(dataPacket, dataPacket.Length, remoteEndPoint);
                Console.WriteLine("DATA SEND");
                currentSequenceNumber++;
            }
            SendFinPacket();


        }


        static void SendDataPacket(byte[] data)
        {
            byte[] dataPacket = CreateDataPacket(data);
            udpClient.Send(dataPacket, dataPacket.Length, remoteEndPoint);
        }

        static void SendFinPacket()
        {
            byte[] finPacket = CreatePacket(0, 0, 1, 0, null);
            udpClient.Send(finPacket, finPacket.Length, remoteEndPoint);
        }

        static void SendRstPacket()
        {
            byte[] rstPacket = CreatePacket(0, 0, 0, 1, null);
            udpClient.Send(rstPacket, rstPacket.Length, remoteEndPoint);
        }

        static ushort GetLastSequenceNumber(byte[] data)
        {
            // last two bytes is seq numb (see if ok ) 
            return BitConverter.ToUInt16(data, data.Length - 2);
        }

        static byte[] CreateDataPacket(byte[] data)
        {
            return CreatePacket(0, 0, 0, 0, data);
        }

        static byte[] CreatePacket(byte synFlag, byte ackFlag, byte finFlag, byte rstFlag, byte[]? data = null)
        {
            byte[] packet = new byte[6 + (data != null ? data.Length : 0)];

            BitConverter.GetBytes(currentSequenceNumber).CopyTo(packet, 0);

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