using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSocket
{
    struct SendBufferPacket
    {
        public int Offset;
        public int Count;
    }

    public class AsyncSendBufferManager
    {
        public DynamicBuffer DynamicBufferManager { get; set; }
        private List<SendBufferPacket> m_SendBufferList;
        private SendBufferPacket m_SendBufferPacket;

        public int Count
        {
            get
            {
                return m_SendBufferList.Count;
            }
        }

        public AsyncSendBufferManager(int bufferSize)
        {
            DynamicBufferManager = new DynamicBuffer(bufferSize);
            m_SendBufferList = new List<SendBufferPacket>();
            m_SendBufferPacket.Offset = 0;
            m_SendBufferPacket.Count = 0;
        }

        public void StartPacket()
        {
            m_SendBufferPacket.Offset = DynamicBufferManager.DataCount;
            m_SendBufferPacket.Count = 0;
        }

        public void EndPacket()
        {
            m_SendBufferPacket.Count = DynamicBufferManager.DataCount - m_SendBufferPacket.Offset;
            m_SendBufferList.Add(m_SendBufferPacket);
        }

        public bool GetFirstPacket(ref int offset, ref int count)
        {
            if (m_SendBufferList.Count <= 0)
                return false;
            offset = 0;
            count = m_SendBufferList[0].Count;
            return true;
        }

        public bool ClearFirstPacket()
        {
            if (m_SendBufferList.Count <= 0)
                return false;
            int count = m_SendBufferList[0].Count;
            DynamicBufferManager.Clear(count);
            m_SendBufferList.RemoveAt(0);
            return true;
        }

        public void ClearPacket()
        {
            m_SendBufferList.Clear();
            DynamicBufferManager.Clear(DynamicBufferManager.DataCount);
        }
    }
}
