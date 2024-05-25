using System.Net;
using System.Net.Sockets;

namespace NetworkToolBox
{
    public static class ToolBox
    {
        public static byte[] CreatePacket(ushort sequenceNumber, byte synFlag, byte ackFlag, byte finFlag, byte rstFlag, byte[]? data = null)
        {
            byte[] packet;
            packet = new byte[36 + (data != null ? data.Length : 0)];

            if (data != null)
            {
                BitConverter.GetBytes(data.Length).CopyTo(packet, 0);
                data.CopyTo(packet, 36);

            }
            BitConverter.GetBytes(sequenceNumber).CopyTo(packet, 16);
            packet[32] = synFlag;
            packet[33] = ackFlag;
            packet[34] = finFlag;
            packet[35] = rstFlag;

            return packet;
        }


        public static void SendAckPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            byte[] ackData = BitConverter.GetBytes(sequenceNumber);
            byte[] ackPacket = CreatePacket(0, 0, 0, 0, 0, ackData);
            udpClient.Send(ackPacket, ackPacket.Length, remoteEndPoint);
        }

        public static void SendSynPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            byte[] synPacket = CreatePacket(sequenceNumber, 1, 0, 0, 0);
            udpClient.Send(synPacket, synPacket.Length, remoteEndPoint);
        }

        public static void SendSynAckPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            byte[] synAckPacket = CreatePacket(sequenceNumber, 1, 1, 0, 0);
            udpClient.Send(synAckPacket, synAckPacket.Length, remoteEndPoint);
        }

        public static void SendFinPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            byte[] finPacket = CreatePacket(sequenceNumber, 0, 0, 1, 0);
            udpClient.Send(finPacket, finPacket.Length, remoteEndPoint);
        }

        public static void SendFinAckPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            byte[] synAckPacket = CreatePacket(sequenceNumber, 0, 1, 1, 0);
            udpClient.Send(synAckPacket, synAckPacket.Length, remoteEndPoint);
        }

        public static void SendRstPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            byte[] finPacket = CreatePacket(sequenceNumber, 0, 0, 1, 0);
            udpClient.Send(finPacket, finPacket.Length, remoteEndPoint);
        }

        public static ushort GetLastSequenceNumber(byte[] data)
        {
            int cpt = 0;
            byte[] byteReaded = new byte[16];

            for (int i = 16; i < 32; i++)
            {
                byteReaded[cpt] = data[i];
                cpt++;
            }
            return (ushort)BitConverter.ToInt16(byteReaded, 0);

        }

        public static void ShowLog(string msg)
        {
            Console.WriteLine(DateTime.Now.TimeOfDay + ">> " + msg);
        }

        public static void OutOfTimeCallBack(object o)
        {
            throw new TimeoutException();
        }
    }
}