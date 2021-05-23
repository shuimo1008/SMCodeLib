using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSocket
{
    public interface IPacket
    {
        int PacketID { get; set; }
        object Owner { get; set; }
        void Serialization(DynamicBuffer oBuffer);
        void Deserialization(DynamicBuffer oBuffer);
    }
}
