using PipelineUDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PipelineUDP_CLIENT
{
    class Program
    {
        public const string IP = "127.0.0.1";
        public const int SendPort = 10089;
        public const int RecvPort = 10088;

        public static PiplineClient Client;


        static void Main(string[] args)
        {
            IPEndPoint sendPoint = new IPEndPoint(IPAddress.Parse(IP), SendPort);
            IPEndPoint recvPoint = new IPEndPoint(IPAddress.Parse(IP), RecvPort);
            Client = new PiplineClient(sendPoint, recvPoint);

            float deltaTime = 0;
            long currentTime = DateTime.Now.Ticks;
            while (true)
            {
                deltaTime = (DateTime.Now.Ticks - currentTime) / 10000.0F / 1000.0F;
                Update(deltaTime);
                currentTime = DateTime.Now.Ticks;
                Thread.Sleep(10);
            }
        }

        const float DELAYTIME = 1.0f;
        static float useTime = 0;
        //const string CONTENT = 

        static void Update(float deltaTime)
        {
            useTime = useTime - deltaTime;
            if (useTime <= 0)
            {
                Client.Send(new UDPPacket() { Content = "当年忠贞为国酬，何曾怕断头？如今天下红遍，江山靠谁守？业未就，身躯倦，鬓已秋。你我之辈，忍将夙愿，付与东流？" });
                useTime = DELAYTIME;

                Console.WriteLine(string.Format("已存发包数:{0}; 已存收包数:{1}", Client.SentCount, Client.LostCount));
            }

            Client.Update(deltaTime);
        }
    }
}
