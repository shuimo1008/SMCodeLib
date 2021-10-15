using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZCSharpLib.Nets;

namespace ZCSharpLib.Nets
{
    public class DynamicBuffer
    {
        struct BufferSize
        {
            public int Offset;
            public int Count;
        }

        public ByteBuffer Buffer { get; private set; }
        private List<BufferSize> BufferSizes { get; set; }

        private BufferSize sendBufferPacket;

        public int Count
        {
            get
            {
                return BufferSizes.Count;
            }
        }

        public DynamicBuffer(int bufferSize)
        {
            Buffer = new ByteBuffer(bufferSize);
            BufferSizes = new List<BufferSize>();
            sendBufferPacket.Count = 0;
            sendBufferPacket.Offset = 0;
        }

        public void StartPacket()
        {
            sendBufferPacket.Count = 0;
            sendBufferPacket.Offset = Buffer.Position;
        }

        public void EndPacket()
        {
            sendBufferPacket.Count = Buffer.Position - sendBufferPacket.Offset;
            BufferSizes.Add(sendBufferPacket);
        }

        public bool GetFirstPacket(ref int offset, ref int count)
        {
            if (BufferSizes.Count <= 0)
                return false;
            offset = 0;//m_sendBufferList[0].Offset;清除了第一个包后，后续的包往前移，因此Offset都为0
            count = BufferSizes[0].Count;
            return true;
        }

        public bool ClearFirstPacket()
        {
            if (BufferSizes.Count <= 0)
                return false;
            int count = BufferSizes[0].Count;
            Buffer.Clear(count);
            BufferSizes.RemoveAt(0);
            return true;
        }

        public void ClearPacket()
        {
            BufferSizes.Clear();
            Buffer.Clear(Buffer.Position);
        }
    }
}
