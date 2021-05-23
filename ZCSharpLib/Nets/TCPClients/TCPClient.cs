using System;
using ZCSharpLib.Times;
using ZCSharpLib.Nets;
using ZCSharpLib.Nets.TCPSockets;

namespace ZCSharpLib.Nets.TCPClients
{
    public class TCPClient
    {
        public enum ConnectStatus
        {
            None, Connecting, Connected, Disconnected,
        }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// 服务器IP
        /// </summary>
        public string IP { get; private set; }
        /// <summary>
        /// 连接超时
        /// </summary>
        public float ConnectTimeout { get; set; }
        /// <summary>
        /// 网络延迟
        /// </summary>
        public int Delay { get { return Heart.Delay; } }
        /// <summary>
        /// 网络状态
        /// </summary>
        public ConnectStatus Status { get; set; }
        /// <summary>
        /// 连接状态回调
        /// </summary>
        public Action<ConnectStatus> OnStatusChanged { get; set; }

        public TCPSocketClient Net { get; set; }
        private float ConnectTime { get; set; }

        public TCPClient(string ip, int port)
        {
            IP = ip;
            Port = port;
            Net = new TCPSocketClient();
            Heart.Client = this;
            ConnectTimeout = 30.0f; // 连接超时时间30秒
            Status = ConnectStatus.Disconnected;
        }

        public void Connect()
        {
            if (Status == ConnectStatus.Disconnected)
            {
                App.SubscribeUpdate(Update);
                Net.Connect(IP, Port);
                ConnectTime = 0;
                Status = ConnectStatus.Connecting;
                OnStatusChanged?.Invoke(Status);
            }
        }

        public void Disconnect()
        {
            if (Status == ConnectStatus.Connected ||
                Status == ConnectStatus.Connecting)

            {
                App.UnsubscribeUpdate(Update);
                Net.CloseSocket(Net.SocketToken);
                ConnectTime = 0;
                Status = ConnectStatus.Disconnected;
            }
        }

        public void Send(IPacket packet)
        {
            if (Net.IsConnected)
            {
               Net.SocketToken.SendAsync(packet);
            }
        }

        public void Update(float deltaTime)
        {
            // 网络状态更新
            if (Status == ConnectStatus.Connecting)
            {
                ConnectTime = ConnectTime + deltaTime;

                // 如果连接超时则关闭连接
                if (ConnectTime >= ConnectTimeout) Disconnect();

                if (Net.IsConnected)
                {
                    Status = ConnectStatus.Connected;
                    OnStatusChanged?.Invoke(Status);
                }
            }

            if (Status == ConnectStatus.Connected)
            {
                if (!Net.IsConnected)
                {
                    Status = ConnectStatus.Disconnected;
                    OnStatusChanged?.Invoke(Status);
                }
            }

            if (Net.IsConnected)
            {
                Heart.Update(deltaTime);
            }

            // 如果延迟大于30000(30秒)毫秒则断开连接
            if (Heart.Delay > 30000) Disconnect();
        }
    }
}