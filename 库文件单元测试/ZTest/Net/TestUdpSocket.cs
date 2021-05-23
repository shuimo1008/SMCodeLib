using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZCSharpLib;
using ZCSharpLib.Log;
using ZCSharpLib.Net;
using ZCSharpLib.Net.Packet;
using ZCSharpLib.Net.Udp;

namespace ZTest.Net
{
    public class TestUdpSocket
    {
        public static void Start()
        {
            UdpSocket oSocket = new UdpSocket();
            oSocket.Start(10086);
            while (true)
            {
                ReqBussinessLogin oReqBussinessLogin = new ReqBussinessLogin()
                {
                    ID = 9999,
                    Name = "景春科技",
                };
                oSocket.UserToken.SendAsync(oReqBussinessLogin);
                App.Singleton<ZLogger>().NETMSG("发送协议消息[{0}]：(ID = {1}, Name = {2})", oReqBussinessLogin.GetType().Name, oReqBussinessLogin.ID, oReqBussinessLogin.Name);
                Thread.Sleep(5000);
            }
        }
    }


    [Protocol(ID = 10005, Name = "BussinessLogin", Type = ProcessType.Send)]
    public class ReqBussinessLogin : BasePacket<ReqBussinessLogin>
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public override void Serialization(DynamicBuffer oBuffer)
        {
            oBuffer.WriteInt32(ID);
            oBuffer.WriteUTF8(Name);
        }

        public override void Deserialization(DynamicBuffer oBuffer)
        {
            ID = oBuffer.ReadInt32();
            Name = oBuffer.ReadUTF8();
        }
    }
}
