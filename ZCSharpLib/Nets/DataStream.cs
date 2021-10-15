using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Nets
{
    public interface IDataStream
    {
        AsyncUserToken UserToken { get; set; }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        byte[] SendStream(byte[] bytes);
        /// <summary>
        /// 数据处理
        /// </summary>
        /// <param name="bytes">传输字节</param>
        /// <param name="offset">字节偏移</param>
        /// <param name="bytesTransferred">字节长度</param>
        /// <returns>成功返回true; 失败返回false,关闭连接.</returns>
        bool RecvStream(byte[] bytes, int offset, int bytesTransferred);
        /// <summary>
        /// 数据流清理
        /// </summary>
        void Clear();
    }

    public abstract class DataStream : IDataStream
    {
        protected int BufferSize { get; set; }
        public AsyncUserToken UserToken { get; set; }

        public DataStream(int bufferSize)
        {
            BufferSize = bufferSize;
        }

        public abstract byte[] SendStream(byte[] bytes);

        public abstract bool RecvStream(byte[] bytes, int offset, int bytesTransferred);

        public abstract void Clear();
    }


    public class DefaultDataStream : DataStream
    {
        /// <summary>包头验证</summary>
        public const short HEAD = 0x6E05;
        /// <summary>字节偏移</summary>
        protected int BytesTransferred { get; set; }
       
        protected ByteBuffer RecvBuffer { get; private set; }

        protected PacketMgr PacketMgr { get; private set; }


        public DefaultDataStream(int bufferSize) 
            : base(bufferSize)
        {
            RecvBuffer = new ByteBuffer(BufferSize);
            PacketMgr = new PacketMgr();
        }

        public override bool RecvStream(byte[] bytes, int offset, int bytesTransferred)
        {
            // 写入数据内容
            int lastPos = RecvBuffer.Position;
            RecvBuffer.WriteBytes(bytes, offset, bytesTransferred);
            RecvBuffer.Clear(RecvBuffer.Position - lastPos); // Position位置回退

            BytesTransferred = BytesTransferred + bytesTransferred;

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

        public override byte[] SendStream(byte[] bytes)
        {
            byte[] stream = new byte[sizeof(short) + sizeof(int) + bytes.Length];
            Array.Copy(BitConverter.GetBytes(HEAD), stream, sizeof(short)); // 包头
            Array.Copy(BitConverter.GetBytes(bytes.Length), 0, stream, sizeof(short), sizeof(int)); // 包长
            Array.Copy(bytes, 0, stream, sizeof(short) + sizeof(int), bytes.Length); // 数据
            return stream;
        }

        public override void Clear() => RecvBuffer.Clear();
    }
}
