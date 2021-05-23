using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZCSharpLib;
using ZCSharpLib.Nets;

namespace ZCSharpLib.Nets.TCPClients
{
    public class Heart
    {
        private static float UseTime { get; set; }
        private static float Interval { get; set; }
        /// <summary>
        /// 总计时
        /// </summary>
        private static int TotalTime { get; set; }
        /// <summary>
        /// 延迟(毫秒)
        /// </summary>
        public static int Delay { get; private set; }
        /// <summary>
        /// 网络工作对象
        /// </summary>
        public static TCPClient Client { get; set; }

        static Heart()
        {
            Interval = 1.0f;
            UseTime = Interval;
        }

        public static void Update(float deltaTime)
        {
            // 发送心跳包
            if (UseTime >= Interval)
            {
                Sendheart();
            }

            // 计时
            UseTime = UseTime - deltaTime;
            if (UseTime <= 0) UseTime = Interval;
            // 毫秒计算
            int millisecond = (int)(deltaTime * 1000);
            TotalTime = TotalTime + millisecond;
        }

        private static void Sendheart()
        {
            Client.Send(new DaemonPacket() { TimeNow = TotalTime });
        }

        public static void Delayed(int timeNow)
        {
            Delay = TotalTime - timeNow; // 延迟
        }
    }

    [Packet(100)]
    public class DaemonPacketCreator : IPacketCreator
    {
        public IPacket CreatePacket()
        {
            return new DaemonPacket();
        }
    }

    [Packet(100)]
    public class DaemonPacket : Packet
    {
        public int TimeNow { get; set; }

        public override void Serialization(ByteBuffer buffer, bool isSerialize)
        {
            PacketID = 100;
            if (isSerialize)
            {
                buffer.WriteInt32(PacketID);
                buffer.WriteInt32(TimeNow);
            }
            else
            {
                PacketID = buffer.ReadInt32();
                TimeNow = buffer.ReadInt32();
            }
        }
    }

    public class DaemonProtocol : Protocol<DaemonPacket>
    {
        public override void Process(DaemonPacket packet)
        {
            AsyncUserToken socketToken = packet.Session as AsyncUserToken;
            if (socketToken.IsClient)
            {
                Heart.Delayed(packet.TimeNow);
            }
            else
            {
                socketToken.SendAsync(new DaemonPacket() { TimeNow = packet.TimeNow  });
            }
        }
    }
}