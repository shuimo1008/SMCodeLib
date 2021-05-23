using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZCSharpLib;
using ZCSharpLib.Nets;
using ZCSharpLib.Nets.TCPSockets;

namespace NetServerDemo
{
    class Program
    {
        public const int MAXCLIENT = 1000;
        public static TCPSocketServer Network { get; set; }

        static void Main(string[] args)
        {
            Network = new TCPSocketServer(MAXCLIENT);
            Network.Listen("192.168.1.254", 10081);

            float deltaTime = 0.0f;
            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                stopwatch.Start();
                App.Update(deltaTime);
                Thread.Sleep(10);
                stopwatch.Stop();
                deltaTime = stopwatch.ElapsedMilliseconds;
            }
        }
    }

    public class Nc2sMessageProtocol : Protocol<Nc2sMessagePacket>
    {
        public override void Process(Nc2sMessagePacket packet)
        {
            AsyncUserToken userToken = packet.Session as AsyncUserToken;
            userToken.SendAsync(new Ns2cMessagePacket()
            {
                ClientID = packet.ClientID,
                MessageID = packet.MessageID,
                message = packet.Message,
                SendTime = packet.SendTime,
            });
        }
    }
}
