using Confluent.SchemaRegistry.Serdes;
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Container;
using System.Net;
using System.Net.Sockets;


namespace NetworkReceiver
{
    class Receiver
    {
        static UdpClient udpClient;
        static IPEndPoint remoteEndPoint;

        static FileStream fileStream;
        static MemoryStream memoryStream;

        static BinaryDecoder binaryDecoder;
        static BinaryReader binaryReader;

        static ushort currentSequenceNumber;

        static Timer timer;
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Receiver <port> <outputFile>");
                Console.ReadLine();
                return;
            }

            int port = int.Parse(args[0]);
            string outputFile = args[1];

            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            currentSequenceNumber = (ushort)new Random().Next(42);

            byte[] dataReceived;
            fileStream = File.OpenWrite(outputFile);
            do
            {
                timer = new Timer(callback: OutOfTimeCallBack, null, 10000, 10000);
                try
                {
                    dataReceived = udpClient.Receive(ref remoteEndPoint);
                }
                catch (SocketException)
                {
                    break;
                }
                Console.Write(DateTime.Now.TimeOfDay + ">> ");
                foreach (byte data in dataReceived)
                    Console.Write(data);
                Console.WriteLine();
                ProcessReceivedData(dataReceived);
                SendAckPacket(GetLastSequenceNumber(dataReceived));
                timer.Dispose();

            }
            while ((dataReceived != null && dataReceived.Length > 0));
            fileStream.Close();
            Console.WriteLine("END");
            Environment.Exit(0);


        }

        static void ProcessReceivedData(byte[] data)
        {
            if (data.Length > 7)
            {
                byte[] dataToRead = new byte[data.Length - 6];
                data.CopyTo(dataToRead, 7);
                memoryStream = new MemoryStream(dataToRead);

                using (memoryStream)
                {
                    binaryReader = new BinaryReader(memoryStream);

                    while (memoryStream.Position < memoryStream.Length)
                    {
                        //TODO Behavior flags
                        byte[] dataRead = binaryReader.ReadBytes(dataToRead.Length);
                        fileStream.Write(dataRead);
                    }
                }
            }

        }
        static void ManageSenderPacket()
        {

            //TODO method to set the behavior when get a packet from sender
            //switch (switch_on)
            //{
            //    default:
            //}
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


        static void SendSynAckPacket(ushort lastSequenceNumber)
        {
            byte[] ackData = BitConverter.GetBytes(lastSequenceNumber);
            byte[] synAckPacket = CreatePacket(currentSequenceNumber, 1, 0, 0, 0,ackData);

            udpClient.Send(synAckPacket, synAckPacket.Length, remoteEndPoint);

        }



        static ushort GetLastSequenceNumber(byte[] data)
        {
            //TODO check for sequence number behavior
            return BitConverter.ToUInt16(data, data.Length - 2);
        }

        static byte[] CreatePacket(ushort sequenceNumber, byte synFlag, byte ackFlag, byte finFlag, byte rstFlag, byte[]? data = null)
        {
            byte[] packet = new byte[6 + (data != null ? data.Length : 0)];

            BitConverter.GetBytes(sequenceNumber).CopyTo(packet, 0);//TODO create class for binary encoder / decoder

            packet[2] = synFlag;
            packet[3] = ackFlag;
            packet[4] = finFlag;
            packet[5] = rstFlag;

            if (data != null)
                data.CopyTo(packet, 6);

            return packet;
        }

        private static void OutOfTimeCallBack(Object o)
        {
            Console.WriteLine("OUT OF TIME");
            udpClient.Close();
            timer = null;
        }

       
    }
}