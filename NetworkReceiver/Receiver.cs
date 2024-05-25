using System.Net;
using System.Net.Sockets;
using NetworkToolBox;

namespace NetworkReceiver
{
    class Receiver
    {
        static UdpClient udpClient;
        static IPEndPoint remoteEndPoint;

        static FileStream fileStream;
        static MemoryStream memoryStream;

        static BinaryReader binaryReader;

        static ushort currentSequenceNumber;
        static bool finalizeConnexion = false;


        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: NetworkReceiver <port> <outputFile>");
                return;
            }

            int port = int.Parse(args[0]);
            string outputFile = args[1];

            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            currentSequenceNumber = (ushort)new Random().Next(42);

            byte[] dataReceived = new byte[36];
            fileStream = File.OpenWrite(outputFile);
            do
            {
                try
                {
                    dataReceived = udpClient.Receive(ref remoteEndPoint);
                    string msg = "";
                    foreach (byte data in dataReceived)
                        msg += data;

                    ToolBox.ShowLog("RECEIVER: "+msg);
                    ProcessReceivedData(dataReceived);
                }
                catch (SocketException)
                {
                    ToolBox.ShowLog("RECEIVER : Timer out of time");
                    break;
                }
                catch (Exception e)
                {
                    ToolBox.ShowLog("RECEIVER: " + "ERROR : " + e.Message);
                    udpClient.SendRstPacket(currentSequenceNumber, remoteEndPoint);
                }

            }
            while ((dataReceived != null && dataReceived.Length > 0) && !finalizeConnexion);
            fileStream.Close();
            ToolBox.ShowLog("RECEIVER: " + "CONNEXION ENDED");
            Environment.Exit(0);


        }

        static void ProcessReceivedData(byte[] data)
        {
            byte[] byteFlag = new byte[4];
            byteFlag[0] = data[32];
            byteFlag[1] = data[33];
            byteFlag[2] = data[34];
            byteFlag[3] = data[35];




            if (data[34] == 1)//FIN
            {
                ToolBox.ShowLog("RECEIVER: " + "Send FINACK Packet");
                udpClient.SendFinAckPacket(ToolBox.GetLastSequenceNumber(data), remoteEndPoint);
                finalizeConnexion = true;

            }
            else if (data[35] == 1)//RST
            {
                ToolBox.ShowLog("RECEIVER: " + "Send RST Packet");
                udpClient.SendRstPacket(ToolBox.GetLastSequenceNumber(data), remoteEndPoint);
                finalizeConnexion = true;
            }
            else if((data[32] == 1 && data[33] == 1))
            {
                ToolBox.ShowLog("RECEIVER: " + "Send SYN-ACK Packet");
                udpClient.SendSynAckPacket(ToolBox.GetLastSequenceNumber(data), remoteEndPoint);
            }
            else if (data[32] == 1 )
            {
                ToolBox.ShowLog("RECEIVER: " + "Send ACK Packet");
                udpClient.SendAckPacket(ToolBox.GetLastSequenceNumber(data), remoteEndPoint);
            }
            else if (data.Length > 35)
            {
                byte[] dataToRead = data.Where(b => Array.IndexOf(data, b) > 35).ToArray<byte>();
                memoryStream = new MemoryStream(dataToRead);
                using (memoryStream)
                {
                    binaryReader = new BinaryReader(memoryStream);

                    while (memoryStream.Position < memoryStream.Length)
                    {
                        byte[] dataRead = binaryReader.ReadBytes(dataToRead.Length);
                        fileStream.Write(dataRead);
                    }
                }
                ToolBox.ShowLog("RECEIVER: " + "Send ACK Packet");
                udpClient.SendAckPacket(ToolBox.GetLastSequenceNumber(data), remoteEndPoint);
            }


        }


    }
}