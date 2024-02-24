using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkToolBox
{
    static class ToolBox
    {
        public static byte[] CreatePacket(ushort sequenceNumber, byte synFlag, byte ackFlag, byte finFlag, byte rstFlag, byte[]? data = null)
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
    }
}