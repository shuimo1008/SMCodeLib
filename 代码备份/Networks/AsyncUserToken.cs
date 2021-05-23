using System;
using System.Net.Sockets;
using System.Threading;
using ZCSharpLib.Networks;
//using ZCSharpLib.Protocols;

namespace ZCSharpLib.Networks
{
    public class AsyncUserToken
    {
        protected Socket mSocket;
        public Socket Socket
        {
            get { return mSocket; }
            set
            {
                // 当有新的连接时清理缓存
                if (mSocket == null) Clear();
                mSocket = value;
                SendEventArgs.AcceptSocket = mSocket;
                RecvEventArgs.AcceptSocket = mSocket;
            }
        }

        protected DateTime mConnectDateTime;
        public DateTime ConnectDateTime
        {
            get { return mConnectDateTime; }
            set { mConnectDateTime = value; }
        }
        protected DateTime mActiveDateTime;
        public DateTime ActiveDateTime
        {
            get { return mActiveDateTime; }
            set { mActiveDateTime = value; }
        }

        public SocketAsyncEventArgs SendEventArgs { get; set; }
        public SocketAsyncEventArgs RecvEventArgs { get; set; }

        /// <summary>
        /// 服务端ID
        /// </summary>
        public ushort OwnerID1 { get; set; }
        /// <summary>
        /// 客户端ID
        /// </summary>
        public ushort OwnerID2 { get; set; }
        /// <summary>
        /// 包工厂
        /// </summary>
        public PacketFactory PacketFactory { get; set; }
        public IAsyncNet AsyncNet { get; private set; }
        protected Packet Packet { get; private set; }
        protected int BufferSize { get; private set; }
        protected ByteArray RecvBuffer { get; private set; }
        protected DynamicBuffer SendBuffer { get; private set; }

        /// <summary>
        /// 字节偏移
        /// </summary>
        protected int BytesTransferred { get; set; }

        protected bool mSendAsync; //标识是否有发送异步事件

        public AsyncUserToken(IAsyncNet asyncNet, int bufferSize)
        {
            AsyncNet = asyncNet;
            BufferSize = bufferSize;

            SendEventArgs = new SocketAsyncEventArgs();
            SendEventArgs.UserToken = this;

            RecvEventArgs = new SocketAsyncEventArgs();
            byte[] oAsyncReceiveBuffer = new byte[BufferSize];
            RecvEventArgs.SetBuffer(oAsyncReceiveBuffer, 0, oAsyncReceiveBuffer.Length);
            RecvEventArgs.UserToken = this;

            Packet = new Packet();

            RecvBuffer = new ByteArray(BufferSize);
            SendBuffer = new DynamicBuffer(BufferSize);
        }

        public bool RecvAsync()
        {
            // 写入数据内容
            int lastPos = RecvBuffer.Position;
            RecvBuffer.WriteBytes(RecvEventArgs.Buffer, RecvEventArgs.Offset, RecvEventArgs.BytesTransferred);
            RecvBuffer.Clear(RecvBuffer.Position - lastPos); // Position位置回退
            BytesTransferred = BytesTransferred + RecvEventArgs.BytesTransferred;

            bool result = true;

            while (BytesTransferred >= sizeof(short))
            {
                // 包头检查
                short head = RecvBuffer.ReadInt16();
                if (head != Packet.HEAD)
                {
                    App.Logger.Error("包头错误, 关闭远程连接!");
                    return false;
                }
                BytesTransferred = BytesTransferred - sizeof(short);

                // 包长
                int length = RecvBuffer.ReadInt32();
                if ((length > 1024 * 1024) | (RecvBuffer.Position > BufferSize))
                {
                    // 最大缓存保护
                    App.Logger.Error("字节流溢出,即将关闭远程连接!");
                    return false;
                }
                BytesTransferred = BytesTransferred - sizeof(int);

                lastPos = RecvBuffer.Position;
                Packet.Serialization(RecvBuffer, false);
                RecvBuffer.Clear(RecvBuffer.Position - lastPos);

                Packet oPacket = PacketFactory.CreatePacket(Packet.PacketID);
                if (oPacket != null)
                {
                    lastPos = RecvBuffer.Position;
                    oPacket.Serialization(RecvBuffer, false);
                    int oDataLength = RecvBuffer.Position - lastPos;
                    BytesTransferred = BytesTransferred - oDataLength;
                    // 接收包
                    oPacket.state = this;
                    AsyncNet.Recive(oPacket);
                }
                else
                {
                    App.Logger.Error("协议解析出错, 即将关闭远程连接");
                    return false;
                }
            }
            return result;
        }

        public bool SendAsync(byte[] bytes)
        {
            SendBuffer.StartPacket();
            // 写入包头
            SendBuffer.Buffer.WriteInt16(Packet.HEAD);
            // 写入包长
            SendBuffer.Buffer.WriteInt32(bytes.Length);          
            // 写入数据
            SendBuffer.Buffer.WriteBytes(bytes, 0, bytes.Length);
            SendBuffer.EndPacket();
            bool result = true;
            if (!mSendAsync)
            {
                int count = 0;
                int offset = 0;
                if (SendBuffer.GetFirstPacket(ref offset, ref count))
                {
                    mSendAsync = true;
                    SendEventArgs.SetBuffer(SendBuffer.Buffer.Bytes, offset, count);
                    result = AsyncNet.SendAsync(this);
                }
            }
            return result;
        }

        public bool SendAsyncCompleted()
        {
            ActiveDateTime = DateTime.UtcNow;
            mSendAsync = false;
            SendBuffer.ClearFirstPacket(); //清除已发送的包
            int offset = 0;
            int count = 0;
            if (SendBuffer.GetFirstPacket(ref offset, ref count))
            {
                mSendAsync = true;
                // 设置发送的数据包
                SendEventArgs.SetBuffer(SendBuffer.Buffer.Bytes, offset, count);
                return AsyncNet.SendAsync(this);
            }
            else
                return true;
        }

        public void Clear()
        {
            mSendAsync = false;
            RecvBuffer.Clear(RecvBuffer.Position);
            SendBuffer.ClearPacket();
        }
    }
}
