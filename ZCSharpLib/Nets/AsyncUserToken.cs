using System;
using System.Net.Sockets;

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

        public SocketAsyncEventArgs SendEventArgs { get; set; }
        public SocketAsyncEventArgs RecvEventArgs { get; set; }

        /// <summary>
        /// 包头验证
        /// </summary>
        public const short HEAD = 0x6E05;
        /// <summary>
        /// 令牌ID
        /// </summary>
        public int TokenID { get; set; }
        /// <summary>
        /// 是否客户端
        /// </summary>
        public bool IsClient { get; set; }
        /// <summary>
        /// 令牌拥有者
        /// </summary>
        public object Owner { get; set; }
        /// <summary>
        /// 包管理
        /// </summary>
        public PacketMgr PacketMgr { get; internal set; }
        /// <summary>
        /// 包工厂
        /// </summary>
        public IAsyncScoket AsyncSocket { get; private set; }
        protected int BufferSize { get; private set; }
        protected ByteBuffer RecvBuffer { get; private set; }
        protected DynamicBuffer SendBuffer { get; private set; }

        /// <summary>
        /// 字节偏移
        /// </summary>
        protected int BytesTransferred { get; set; }

        protected bool isSending; //标识是否有发送异步事件

        private object sync = new object(); // 同步锁

        public AsyncUserToken(IAsyncScoket asyncNet, int bufferSize)
        {
            AsyncSocket = asyncNet;
            BufferSize = bufferSize;

            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.UserToken = this;

            RecvEventArgs = new SocketAsyncEventArgs();
            byte[] oAsyncReceiveBuffer = new byte[BufferSize];
            RecvEventArgs.SetBuffer(oAsyncReceiveBuffer, 0, oAsyncReceiveBuffer.Length);
            RecvEventArgs.UserToken = this;

            RecvBuffer = new ByteBuffer(BufferSize);
            SendBuffer = new DynamicBuffer(BufferSize);
        }

        public bool RecvAsync()
        {
            lock (sync)
            {
                // 写入数据内容
                int lastPos = RecvBuffer.Position;
                RecvBuffer.WriteBytes(RecvEventArgs.Buffer, RecvEventArgs.Offset, RecvEventArgs.BytesTransferred);
                RecvBuffer.Clear(RecvBuffer.Position - lastPos); // Position位置回退

                BytesTransferred = BytesTransferred + RecvEventArgs.BytesTransferred;

                bool result = true;

                int byteLenght = 0;
                // 当字节长度大于一个包头时进行处理
                while (BytesTransferred >= sizeof(short))
                {
                    // 包头检查
                    short head = RecvBuffer.ReadInt16();
                    if (head != HEAD)
                    {
                        App.Error("包头错误, 关闭远程连接!");
                        return false;
                    }
                    BytesTransferred = BytesTransferred - sizeof(short);
                    byteLenght = byteLenght + sizeof(short);

                    // 包长
                    int length = RecvBuffer.ReadInt32();
                    if ((length > 1024 * 1024) | (RecvBuffer.Position > BufferSize))
                    {
                        // 最大缓存保护
                        App.Error("字节流溢出,即将关闭远程连接!");
                        return false;
                    }
                    BytesTransferred = BytesTransferred - sizeof(int);
                    byteLenght = byteLenght + sizeof(int);

                    lastPos = RecvBuffer.Position;

                    int packetID = RecvBuffer.ReadInt32();
                    RecvBuffer.Clear(RecvBuffer.Position - lastPos);

                    IPacket oPacket = PacketMgr.CreatePacket(packetID);
                    try
                    {
                        lastPos = RecvBuffer.Position;
                        oPacket.Serialization(RecvBuffer, false);
                        int oDataLength = RecvBuffer.Position - lastPos;
                        BytesTransferred = BytesTransferred - oDataLength;
                        byteLenght = byteLenght + oDataLength;
                        // 接收包
                        oPacket.SetOwner(Owner);
                        oPacket.SetSession(this);
                        PacketMgr.Push(oPacket);
                    }
                    catch (Exception e)
                    {
                        App.Error("协议解析出错, 即将关闭远程连接\n{0}", e);
                        return false;
                    }
                }

                RecvBuffer.Clear(byteLenght);

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
                SendBuffer.StartPacket();
                // 写入包头
                SendBuffer.Buffer.WriteInt16(HEAD);
                // 写入包长
                SendBuffer.Buffer.WriteInt32(bytes.Length);
                // 写入数据
                SendBuffer.Buffer.WriteBytes(bytes, 0, bytes.Length);
                SendBuffer.EndPacket();
                return SendAsync(false);
            }
        }

        public bool SendAsync(bool isFinished)
        {
            lock (sync)
            {
                if (!isFinished)
                {
                    if (!isSending)
                        return SendData();
                    return true;
                }
                else
                {
                    ActiveDateTime = DateTime.UtcNow;
                    isSending = false;
                    SendBuffer.ClearFirstPacket(); //清除已发送的包
                    return SendData();
                }
            }
        }

        private bool SendData()
        {
            int count = 0;
            int offset = 0;
            if (SendBuffer.GetFirstPacket(ref offset, ref count))
            {
                isSending = true;
                SendEventArgs.SetBuffer(SendBuffer.Buffer.Bytes, offset, count);
                return AsyncSocket.SendAsync(this);
            }
            return true;
        }

        public void Clear()
        {
            isSending = false;
            RecvBuffer.Clear(RecvBuffer.Position);
            SendBuffer.ClearPacket();
        }
    }
}
