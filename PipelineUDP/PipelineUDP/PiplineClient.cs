using PipelineUDP.Jsons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PipelineUDP
{
    public class PiplineClient
    {
        private UdpClient RecvClient { get; set; }
        private UdpClient SendClient { get; set; }
        private Queue<string> RecvMsgs { get; set; }

        private IPEndPoint mSendPoint;
        private IPEndPoint mRecvPoint;

        private PacketFactory PakcetFactory { get; set; }
        private PacketProcess PacketProcess { get; set; }

        /// <summary>
        /// 接收的包序号
        /// </summary>
        private long RecvSN = 0;
        /// <summary>
        /// 丢失的包序号
        /// </summary>
        private List<long> LostRecv { get; set; }

        /// <summary>
        /// 发送的包序号
        /// </summary>
        private long SendSN = 0;
        /// <summary>
        /// 包超时重发
        /// </summary>
        private float TimeOut { get; set; }
        /// <summary>
        /// 超时重发缓存的包
        /// </summary>
        private List<Packet> BeenSent { get; set; }

        /// <summary>
        /// 已存丢包数
        /// </summary>
        public int LostCount { get { return LostRecv.Count; } }
        /// <summary>
        /// 已存发包数
        /// </summary>
        public int SentCount { get { return BeenSent.Count; } }


        public PiplineClient(IPEndPoint sendPoint, IPEndPoint recvPoint, float timeOut = 1.0f)
        {
            mRecvPoint = recvPoint;
            mSendPoint = sendPoint;
            SendClient = new UdpClient();
            RecvClient = new UdpClient(mRecvPoint);

            RecvMsgs = new Queue<string>();

            LostRecv = new List<long>();
            BeenSent = new List<Packet>();

            PakcetFactory = new PacketFactory();
            PacketProcess = new PacketProcess();

            TimeOut = timeOut;

            // 开始接收数据
            RecvClient.BeginReceive(CallBackRecvive, RecvClient);
        }

        private void CallBackRecvive(IAsyncResult asyncResult)
        {
            UdpClient c = asyncResult.AsyncState as UdpClient;
            byte[] recvBuf = c.EndReceive(asyncResult, ref mRecvPoint);
            lock (RecvMsgs)
            {
                RecvMsgs.Enqueue(Encoding.UTF8.GetString(recvBuf));
            }
            c.BeginReceive(CallBackRecvive, c);
        }

        public void Update(float deltaTime)
        {
            lock (RecvMsgs)
            {
                if (RecvMsgs.Count > 0)
                {
                    string oRecvMsg = RecvMsgs.Dequeue();
                    Recv(oRecvMsg);
                }
            }

            // 包验证超时重发
            for (int i = 0; i < BeenSent.Count; i++)
            {
                Packet packet = BeenSent[i];
                packet.wait = packet.wait + deltaTime;
                if (packet.wait > TimeOut)
                {
                    Send(packet, true);
                }
            }
        }

        private void Recv(string msg)
        {
            // 解析包Token
            Packet packet = JsonUtility.Decode<Packet>(msg);

            if (packet.ack == 0)
            {
                // 收到新的数据包后发送回包
                Send(packet, false, true);

                bool isNew = true; // 判断是否新的数据包

                // 接收到的序号
                if (packet.sn >= RecvSN + 1)
                {
                    // 说明:
                    // 1.序号等于现有序号加一说明没有跳包正常接收.
                    // 2.序号大于现有序号加一说明存在丢包,把丢失的包信息保存起来.

                    // 添加丢失的包序号
                    long recv = RecvSN + 1;
                    long recvsn = packet.sn;
                    for (long i = recv; i < recvsn; i++)
                        LostRecv.Add(i);
                    // 当前接收到的包的序号
                    RecvSN = packet.sn;
                }
                else
                {
                    // 说明:当回包验证没法完成时,客户端会重新发包,这时如果已经处理了该包则不用再次处理!

                    // 接收到的序号小于等于现有序号表示该数据包已经接收过或上回丢失的数据包。
                    long recv = -1;
                    recv = LostRecv.Find((it) => it == packet.sn);
                    // 没有找到丢失的包序号则判定该包是客户端重复发送的数据包
                    if (recv == -1) isNew = false;
                }

                if (isNew)
                {
                    // 处理新包
                    Packet newPacket = PakcetFactory.Create(packet.token, msg);
                    if (newPacket != null) PacketProcess.Process(newPacket);
                }

            }
            else
            {
                // 收到回包后删除发送成功的数据包
                BeenSent.RemoveAll((it) => it.sn == packet.sn);
            }
        }
        
        public void Send(Packet packet, bool isAgain = false, bool isAck = false)
        {
            if (!isAck)
            {
                object[] objs = packet.GetType().GetCustomAttributes(typeof(PacketAttribute), false);
                if (objs.Length > 0)
                {
                    if (!isAgain)
                    {
                        // 如果不是再次发送则添加进已经发送队列,
                        // 已经发送的数据已经在队列中,无需重复添加。
                        Interlocked.Increment(ref SendSN);
                        packet.sn = SendSN;
                        BeenSent.Add(packet);
                    }
                    PacketAttribute oAttr = objs[0] as PacketAttribute;
                    packet.token = oAttr.Token;
                }
                packet.ack = 0;  // 设置不是回包
            }
            else packet.ack = 1;

            packet.wait = 0; // 包超时重置
            if (!string.IsNullOrEmpty(packet.token))
            {
                string json = JsonUtility.Encode(packet);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                SendClient.Send(bytes, bytes.Length, mSendPoint);
            }
        }
    }
}