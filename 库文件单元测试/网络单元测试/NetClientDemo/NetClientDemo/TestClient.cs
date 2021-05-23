using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ZCSharpLib.Nets.TCPClients;
using ZCSharpLib.Nets.TCPSockets;

namespace NetClientDemo
{
    public class TestClient
    {
        private int Port { get; set; }
        private string IP { get; set; }
        public int ID { get; private set; }
        private bool IsConnected { get; set; }
        private TCPSocketClient Network { get; set; }

        private int MessageID { get; set; }
        private List<int> MessageIDs { get; set; }

        private object sync = new object();

        public TestClient(int id, string ip, int port)
        {
            ID = id;
            IP = ip;
            Port = port;
            MessageID = 1;
            MessageIDs = new List<int>();
            Network = new TCPSocketClient();
            Network.Closed = (userToken) =>
            {
                IsConnected = false;
            };
            Network.Connected = (userToken) =>
            {
                IsConnected = true;
            };
        }

        public void Connect()
        {
            ThreadPool.QueueUserWorkItem((state)=>
            {
                Network.Connect(IP, Port);
                while (true)
                {
                    Update();
                    // 睡眠10毫秒
                    Thread.Sleep(10);
                }
            });
        }

        private void Update()
        {
            if (IsConnected)
            {
                Network.SocketToken.SendAsync(new Nc2sMessagePacket()
                {
                    ClientID = ID,
                    MessageID = MessageID,
                    Message = "星沉月落夜闻香，素手出锋芒。前缘再续新曲，心有意，爱无伤。江湖远，碧空长，路茫茫，闲愁滋味，多感情怀，无限思量！"
                });
                lock (sync)
                {
                    MessageIDs.Add(MessageID);
                }
                MessageID += 1;
            }
        }

        public void Recv(Ns2cMessagePacket packet)
        {
            lock (sync)
            {
                MessageIDs.Remove(packet.MessageID);
            }
        }
    }
}