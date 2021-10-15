using System;
using System.Net.Sockets;
using ZCSharpLib.Utils;

namespace ZCSharpLib.Nets
{
    public class AsyncUserToken
    {
        protected Socket socket;
        public Socket Socket
        {
            get { return socket; }
            set
            {
                // 当有新的连接时清理缓存
                if (socket == null) Clear();
                socket = value;
                SendEventArgs.AcceptSocket = socket;
                RecvEventArgs.AcceptSocket = socket;
            }
        }

        protected DateTime connectDateTime;
        public DateTime ConnectDateTime
        {
            get { return connectDateTime; }
            set { connectDateTime = value; }
        }
        protected DateTime activeDateTime;
        public DateTime ActiveDateTime
        {
            get { return activeDateTime; }
            set { activeDateTime = value; }
        }


        public int SessionID { get; set; }
        protected int BufferSize { get; private set; }
        protected IDataStream DataStream { get; private set; }
        protected IAsyncScoket AsyncSocket { get; private set; }
        protected DynamicBuffer DynamicBuffer { get; private set; }

        public SocketAsyncEventArgs SendEventArgs { get; set; }
        public SocketAsyncEventArgs RecvEventArgs { get; set; }

        private object sync = new object(); // 同步锁

        public AsyncUserToken(IAsyncScoket asyncSocket, int bufferSize)
        {
            BufferSize = bufferSize;
            AsyncSocket = asyncSocket;

            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.UserToken = this;

            RecvEventArgs = new SocketAsyncEventArgs();
            byte[] oAsyncReceiveBuffer = new byte[BufferSize];
            RecvEventArgs.SetBuffer(oAsyncReceiveBuffer, 0, oAsyncReceiveBuffer.Length);
            RecvEventArgs.UserToken = this;

            DynamicBuffer = new DynamicBuffer(BufferSize);
        }

        public void Bind<T>()
            where T : class, IDataStream
        {
            DataStream = ReflectionUtils.Construct(typeof(T), BufferSize) as T;
            DataStream.UserToken = this;
        }

        public bool RecvAsync()
        {
            lock (sync)
            {
                bool result = false;
                try{
                    result = DataStream.RecvStream(RecvEventArgs.Buffer, RecvEventArgs.Offset, RecvEventArgs.BytesTransferred);
                }
                catch (Exception e){
                    App.Error("协议解析出错, 即将关闭远程连接\n{0}", e);
                }
                return result;
            }
        }

        public void SendAsync(IPacket packet)
        {
            ByteBuffer buffer = new ByteBuffer();
            packet.Serialization(buffer, true);
            SendAsync(buffer.Bytes);
        }

        public bool SendAsync(byte[] bytes)
        {
            lock (sync)
            {
                DynamicBuffer.StartPacket();
                byte[] stream = DataStream.SendStream(bytes);
                DynamicBuffer.Buffer.WriteBytes(stream);
                DynamicBuffer.EndPacket();
                return SendAsync(false);
            }
        }

        protected bool isSending; //标识是否有发送异步事件
        public bool SendAsync(bool isRaiseEvent)
        {
            lock (sync)
            {
                if (!isRaiseEvent)
                {
                    bool isSucess = true;
                    if (!isSending) 
                        isSucess = SendData();
                    return isSucess;
                }
                else
                {
                    isSending = false;
                    ActiveDateTime = DateTime.UtcNow;
                    DynamicBuffer.ClearFirstPacket(); //清除已发送的包
                    return SendData();
                }
            }
        }

        private bool SendData()
        {
            int count = 0;
            int offset = 0;
            if (DynamicBuffer.GetFirstPacket(ref offset, ref count))
            {
                isSending = true; // 数据正在发送中
                SendEventArgs.SetBuffer(DynamicBuffer.Buffer.Bytes, offset, count);
                return AsyncSocket.SendAsync(this);
            }
            return true;
        }

        public void Clear()
        {
            isSending = false;
            DataStream.Clear();
            //RecvBuffer.Clear(RecvBuffer.Position);
            DynamicBuffer.ClearPacket();
        }
    }
}
