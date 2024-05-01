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

        static Timer timer;
        static bool isOutOfTime;
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

            byte[] dataReceived;
            isOutOfTime = false;
            fileStream = File.OpenWrite(outputFile);
            timer = new Timer(OutOfTimeCallBack, null, 0, 30000);
            do
            {
                dataReceived = udpClient.Receive(ref remoteEndPoint);
                timer = new Timer(OutOfTimeCallBack, null, 0, 5000);
                Console.Write(DateTime.Now.TimeOfDay + ">> ");
                foreach (byte data in dataReceived)
                    Console.Write(data);
                Console.WriteLine();
                ProcessReceivedData(dataReceived);
                SendAckPacket(GetLastSequenceNumber(dataReceived));

            }
            while ((dataReceived != null && dataReceived.Length > 0) || isOutOfTime);
            fileStream.Close();
            Console.WriteLine("END");
            Console.ReadLine();
            #region AVRO

            //using (memoryStream)
            //{
            //    binaryDecoder = new BinaryDecoder(memoryStream);

            //    using (var dataReader = AvroContainer.CreateGenericReader(memoryStream))
            //    {
            //        while (dataReader.MoveNext())
            //        {
            //            var dataRead = dataReader.Current;

            //        }
            //    }

            //}
            #endregion
            //ProcessReceivedData(data);
            //SendAckPacket(GetLastSequenceNumber(data));


        }

        static void ProcessReceivedData(byte[] data)
        {
            if (data.Length > 7)
            {
                memoryStream = new MemoryStream(data);

                using (memoryStream)
                {
                    binaryReader = new BinaryReader(memoryStream);

                    while (memoryStream.Position < memoryStream.Length)
                    {
                        //TODO Behavior flags
                        byte[] dataRead = binaryReader.ReadBytes(data.Length);
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
            isOutOfTime = true;
        }

        enum StateFlag
        {
            ON_LISTEN,
            ON_RECEIVED,
            ON_SENT,
            ON_STOP
        }
    }
}