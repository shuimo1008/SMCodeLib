using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZCSharpLib;
using ZCSharpLib.Log;
using ZCSharpLib.Net;
using ZCSharpLib.Net.AsyncSocketCore;
using ZCSharpLib.Net.Interface;
using ZCSharpLib.Net.Packet;
using ZCSharpLib.Net.Protocol;
using ZCSharpLib.Net.Tcp;

namespace ZTest
{
    public class TestSocketServer
    {
        public static void Start()
        {
            SocketServer oServer = new SocketServer(8000);
            oServer.Start("127.0.0.1", 9999);
        }
    }

    [Protocol(ID = 10001, Name = "UserLogin", Type = ProcessType.Process)]
    public class ProcessUserLogin : ProcessProtocol<ResUserLogin>
    {
        public override void Process(object sender, ResUserLogin packet)
        {
            App.Singleton<ZLogger>().INFO("接收到注册数据 ID：{0}， Name：{1}， Password：{2}", packet.UserID, "", "");

            AsyncSocketUserToken oUserToken = sender as AsyncSocketUserToken;
            oUserToken.SendAsync(new ReqUserLogin() { UserID = packet.UserID });
            App.Singleton<ZLogger>().INFO("发送注册数据 ID：{0}， Name：{1}， IsSucess：{2}", packet.UserID, "", true);
        }
    }

    [Protocol(ID = 10001, Name = "UserLogin", Type = ProcessType.Send)]
    public class ReqUserLogin : BasePacket<ReqUserLogin>
    {
        public int UserID;

        public override void Serialization(DynamicBuffer oBuffer)
        {
            oBuffer.WriteInt32(UserID);
        }

        public override void Deserialization(DynamicBuffer oBuffer)
        {
            UserID = oBuffer.ReadInt32();
        }
    }

    [Protocol(ID = 10001, Name = "UserLogin", Type = ProcessType.Receive)]
    public class ResUserLogin : BasePacket<ResUserLogin>
    {
        public int UserID;

        public override void Serialization(DynamicBuffer oBuffer)
        {
            oBuffer.WriteInt32(UserID);
        }

        public override void Deserialization(DynamicBuffer oBuffer)
        {
            UserID = oBuffer.ReadInt32();
        }
    }
}
