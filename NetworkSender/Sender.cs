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
        static bool finalizeConnexion = false;
        static Thread cptThread;
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: NetworkSender.exe <receiverIp> <receiverPort> <inputFile>");
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


            byte[] buffer = new byte[1024];
            int bytesRead;

            //OPEN CONNECTION
            udpClient.SendSynPacket(currentSequenceNumber, remoteEndPoint);

            currentSequenceNumber++;

            int cdwStartConnection = 0;
            while (cdwStartConnection < 3)
            {
                try
                {

                    byte[] synAckData = udpClient.Receive(ref remoteEndPoint);
                    int synAckSequenceNumber = ToolBox.GetLastSequenceNumber(synAckData);
                    cdwStartConnection = 3;

                }
                catch (SocketException)
                {
                    cdwStartConnection++;
                }
                catch (Exception e)
                {
                    ToolBox.ShowLog("SENDER : ERROR : " + e.Message);
                    udpClient.SendRstPacket(currentSequenceNumber, remoteEndPoint);
                    udpClient.Close();
                }
            }

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0 || finalizeConnexion)
            {

                byte[] dataToSend = new byte[bytesRead];
                Array.Copy(buffer, dataToSend, bytesRead);

                byte[] dataPacket = ToolBox.CreatePacket(currentSequenceNumber, 0, 0, 0, 0, dataToSend);
                string msg = "SENDER : ";
                foreach (byte b in dataPacket)
                    msg += b;
                ToolBox.ShowLog(msg);
                udpClient.Send(dataPacket, dataPacket.Length, remoteEndPoint);
                currentSequenceNumber++;
            }
            udpClient.SendFinPacket(currentSequenceNumber, remoteEndPoint);
            ProcessReceivedData(udpClient.Receive(ref remoteEndPoint));


        }

        static void ProcessReceivedData(byte[] data)
        {
            byte[] byteFlag = new byte[4];

            if (data[33] == 1 && data[34] == 1)//FIN
            {
                ToolBox.ShowLog("SENDER: " + "CONNEXION ENDED");
                finalizeConnexion = true;

            }
            else if (data[35] == 1)//RST
            {
                ToolBox.ShowLog("SENDER: " + "Send RST Packet");
                udpClient.SendRstPacket(ToolBox.GetLastSequenceNumber(data), remoteEndPoint);
                finalizeConnexion = true;
            }
            else if ((data[32] == 1 && data[33] == 1) || data[32] == 1)
            {
                ToolBox.ShowLog("SENDER: " + "Send ACK Packet");
                udpClient.SendAckPacket(ToolBox.GetLastSequenceNumber(data), remoteEndPoint);
            }


        }
    }
}