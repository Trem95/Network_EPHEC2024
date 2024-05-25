using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
                data.CopyTo(packet, 35);
            }

            BitConverter.GetBytes(sequenceNumber).CopyTo(packet, 0);
            packet[31] = synFlag;
            packet[32] = ackFlag;
            packet[33] = finFlag;
            packet[34] = rstFlag;

            return packet;
        }

        public static byte[] DataToBinary(FileStream fileStream)
        {
            //TODO
            return new byte[0];
        }

        public static byte[] BinaryToData(byte[] binary)
        {
            //TODO
            return new byte[0];
        }

        public static void SendAckPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            ShowLog("Send ACK Packet");
            byte[] ackData = BitConverter.GetBytes(sequenceNumber);
            byte[] ackPacket = CreatePacket(0, 0, 0, 0, 0, ackData);
            udpClient.Send(ackPacket, ackPacket.Length, remoteEndPoint);
        }

        public static void SendSynPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            ShowLog("Send SYN Packet");
            byte[] synPacket = CreatePacket(sequenceNumber, 1, 0, 0, 0);
            udpClient.Send(synPacket, synPacket.Length, remoteEndPoint);
        }

        public static void SendSynAckPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            ShowLog("Send SYN-ACK Packet");
            byte[] synAckPacket = CreatePacket(sequenceNumber, 1, 1, 0, 0);
            udpClient.Send(synAckPacket, synAckPacket.Length, remoteEndPoint);
        }

        public static void SendFinPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            ShowLog("Send FIN Packet");
            byte[] finPacket = CreatePacket(sequenceNumber, 0, 0, 1, 0);
            udpClient.Send(finPacket, finPacket.Length, remoteEndPoint);
        }

        public static void SendRstPacket(this UdpClient udpClient, ushort sequenceNumber, IPEndPoint remoteEndPoint)
        {
            ShowLog("Send RST Packet");
            byte[] finPacket = CreatePacket(sequenceNumber, 0, 0, 1, 0);
            udpClient.Send(finPacket, finPacket.Length, remoteEndPoint);
        }

        public static ushort GetLastSequenceNumber(byte[] data)
        {
            List<byte> reader = new List<byte>();
            for (int i = 16; i <31; i++)
            {
                reader.Add(data[i]);
            }

            return BitConverter.ToUInt16(data, data.Length - 2);
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