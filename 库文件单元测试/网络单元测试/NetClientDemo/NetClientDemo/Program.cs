using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ZCSharpLib;
using ZCSharpLib.Comms;
using ZCSharpLib.Events;
using ZCSharpLib.Guids;
using ZCSharpLib.Nets;
using ZCSharpLib.Objects;

namespace NetClientDemo
{
    class Program
    {
        private const int MAXCLIENT = 200;
        private const int MAXSENDCOUNT = 1000;

        public static Dictionary<int, TestClient> Networks { get; set; }

        static void Main(string[] args)
        {
            Networks = new Dictionary<int, TestClient>();

            int startIndex = (int)DateTime.Now.Ticks;
            Console.WriteLine("客户端开始ID：" + startIndex);
            for (int i = 0; i < MAXCLIENT; i++)
            {
                int clientID = startIndex + i;
                Networks.Add(clientID, new TestClient(clientID, "192.168.1.254", 10081));
            }
            // 开始连接
            foreach (var net in Networks.Values)
                net.Connect();

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

    public class Ns2cProtocol : Protocol<Ns2cMessagePacket>
    {
        public override void Process(Ns2cMessagePacket packet)
        {
            Console.WriteLine($"ClientID={packet.ClientID} MessageID={packet.MessageID}  {packet.message}");
            TestClient client;
            if (Program.Networks.TryGetValue(packet.ClientID, out client))
                client.Recv(packet);
        }
    }

}
