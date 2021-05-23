using System;
using System.Net.Sockets;

namespace NetSocket
{
    public class AsyncSocketUserToken
    {
        protected Socket m_Socket;
        public Socket ConnectSocket
        {
            get
            {
                return m_Socket;
            }
            set
            {
                if (m_Socket == null)
                {
                    m_SendAsync = false;
                    ReceiveBuffer.Clear(ReceiveBuffer.DataCount);
                    SendBufferMgr.ClearPacket();
                }

                m_Socket = value;
                SendEventArgs.AcceptSocket = m_Socket;
                ReceiveEventArgs.AcceptSocket = m_Socket;
            }
        }

        private bool m_SendAsync;
        public ITokenOwner Owner { get; set; }
        public SocketAsyncEventArgs SendEventArgs { get; set; }
        public SocketAsyncEventArgs ReceiveEventArgs { get; set; }
        public IAsyncSocket AsyncSocket { get; set; }
        public ProtocolMgr ProtocolMgr { get; set; }

        protected DateTime m_ConnectDateTime;
        public DateTime ConnectDateTime { get { return m_ConnectDateTime; } set { m_ConnectDateTime = value; } }
        protected DateTime m_ActiveDateTime;
        public DateTime ActiveDateTime { get { return m_ActiveDateTime; } set { m_ActiveDateTime = value; } }
        public DynamicBuffer ReceiveBuffer { get; private set; }
        private AsyncSendBufferManager SendBufferMgr { get; set; }

        public AsyncSocketUserToken(int asyncReceiveBufferSize)
        {
            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.UserToken = this;
            ReceiveEventArgs = new SocketAsyncEventArgs();
            byte[] oAsyncReceiveBuffer = new byte[asyncReceiveBufferSize];
            ReceiveEventArgs.SetBuffer(oAsyncReceiveBuffer, 0, oAsyncReceiveBuffer.Length);
            ReceiveEventArgs.UserToken = this;
            ReceiveBuffer = new DynamicBuffer();
            SendBufferMgr = new AsyncSendBufferManager(asyncReceiveBufferSize);
        }

        public AsyncSocketUserToken Setup(ITokenOwner owner)
        {
            Owner = owner;
            return this;
        }

        public void TokenClose()
        {
            if (Owner != null)
            {
                Owner.TokenClose();
            }
        }

        internal bool Receive(byte[] buffer, int offset, int count)
        {
            ReceiveBuffer.WriteBytes(buffer, offset, count);
            bool result = true;
            while (ReceiveBuffer.DataCount > sizeof(int))
            {
                int packetLength = BitConverter.ToInt32(ReceiveBuffer.Buffer, 0);

                if ((packetLength > 10 * 1024 * 1024) | (buffer.Length > 10 * 1024 * 1024))
                {
                    Console.WriteLine("Memory out , will close remote connect");
                    return false;
                }

                if ((ReceiveBuffer.DataCount - sizeof(int)) >= packetLength)
                {
                    if (ProtocolMgr != null)
                    {
                        result = ProtocolMgr.Process(this, ReceiveBuffer.Buffer, sizeof(int), packetLength);
                        if (result)
                        {
                            ReceiveBuffer.Clear(packetLength + sizeof(int));
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        return result;
                    }
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        public bool SendAsync(IPacket oPacket)
        {
            DynamicBuffer sendBuffer = new DynamicBuffer();
            oPacket.Serialization(sendBuffer);
            int totalLength = sizeof(int) + sendBuffer.DataCount;
            SendBufferMgr.StartPacket();
            SendBufferMgr.DynamicBufferManager.WriteInt32(totalLength);
            SendBufferMgr.DynamicBufferManager.WriteInt32(ProtocolMgr.GetPacketID(oPacket));
            SendBufferMgr.DynamicBufferManager.WriteBytes(sendBuffer.Buffer);
            SendBufferMgr.EndPacket();

            bool result = true;
            if (!m_SendAsync)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (SendBufferMgr.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    m_SendAsync = true;
                    result = AsyncSocket.SendAsync(ConnectSocket, SendEventArgs, SendBufferMgr.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }

        public bool SendAsyncCompleted()
        {
            ActiveDateTime = DateTime.UtcNow;
            m_SendAsync = false;
            SendBufferMgr.ClearFirstPacket();
            int offset = 0;
            int count = 0;
            if (SendBufferMgr.GetFirstPacket(ref offset, ref count))
            {
                m_SendAsync = true;
                return AsyncSocket.SendAsync(ConnectSocket, SendEventArgs,
                    SendBufferMgr.DynamicBufferManager.Buffer, offset, count);
            }
            else
                return SendCallback();
        }

        public virtual bool SendCallback()
        {
            return true;
        }
    }
}
