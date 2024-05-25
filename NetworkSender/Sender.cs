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
                        ushort synAckSequenceNumber = ToolBox.GetLastSequenceNumber(synAckData);
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
            udpClient.SendFinPacket(currentSequenceNumber,remoteEndPoint);


        }



        static byte[] CreateDataPacket(byte[] data)
        {
            return ToolBox.CreatePacket(currentSequenceNumber,0, 0, 0, 0, data);
        }



    }

}