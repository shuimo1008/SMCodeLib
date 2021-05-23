using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Networks
{
    public class Packet
    {
        public const short HEAD = 0x6E05;

        public ushort OwnerID1;
        public ushort OwnerID2;
        public int PacketID;
        public object state;

        public virtual void Serialization(ByteArray byteArray, bool isSerialize)
        {
            if (isSerialize)
            {
                byteArray.WriteUInt16(OwnerID1);
                byteArray.WriteUInt16(OwnerID2);
                byteArray.WriteInt32(PacketID);
            }
            else
            {
                OwnerID1 = byteArray.ReadUInt16();
                OwnerID2 = byteArray.ReadUInt16();
                PacketID = byteArray.ReadInt32();
            }
        }
    }
}
