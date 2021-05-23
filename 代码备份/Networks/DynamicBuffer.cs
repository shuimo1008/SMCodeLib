using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZCSharpLib.Networks;

namespace ZCSharpLib.Networks
{
    /// <summary>
    /// 由于是异步发送，有可能接收到两个命令，
    /// 写入了两次返回，发送需要等待上一次回调才发下一次的响应
    /// </summary>
    public class DynamicBuffer
    {
        struct BufferSize
        {
            public int Offset;
            public int Count;
        }

        public ByteArray Buffer { get; private set; }
        private List<BufferSize> BufferSizes { get; set; }

        private BufferSize mSendBufferPacket;

        public int Count
        {
            get
            {
                return BufferSizes.Count;
            }
        }

        public DynamicBuffer(int bufferSize)
        {
            Buffer = new ByteArray(bufferSize);
            BufferSizes = new List<BufferSize>();
            mSendBufferPacket.Count = 0;
            mSendBufferPacket.Offset = 0;
        }

        public void StartPacket()
        {
            mSendBufferPacket.Count = 0;
            mSendBufferPacket.Offset = Buffer.Position;
        }

        public void EndPacket()
        {
            mSendBufferPacket.Count = Buffer.Position - mSendBufferPacket.Offset;
            BufferSizes.Add(mSendBufferPacket);
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
