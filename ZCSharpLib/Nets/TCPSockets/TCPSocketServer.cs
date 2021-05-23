using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using ZCSharpLib.Cores;

namespace ZCSharpLib.Nets.TCPSockets
{
    public class TCPSocketServer : IAsyncScoket
    {
        /// <summary>
        /// 超时连接(默认1000毫秒=1分钟)
        /// </summary>
        public int SocketTimeOutMS { get; set; }
        /// <summary>
        /// Socket连接
        /// </summary>
        protected Socket ListenSocket { get; set; }
        /// <summary>
        /// 超时守护线程
        /// </summary>
        protected TCPSocketServerDaemonThread DaemonThread { get; set; }
        /// <summary>
        /// 每个连接接收缓存大小
        /// </summary>
        protected int BufferSize { get; set; }
        /// <summary>
        /// 最大支持连接个数
        /// </summary>
        protected int NumConnections { get; set; }
        /// <summary>
        /// 限制访问接收连接的线程数，用来控制最大并发数
        /// </summary>
        protected Semaphore MaxNumberAccepted { get; set; }
        protected List<AsyncUserToken> AsyncSocketUserTokenUsed { get; set; }
        protected ObjectPool<AsyncUserToken> AsyncSocketUserTokenPool { get; set; }

        /// <summary>
        /// 包管理
        /// </summary>
        public PacketMgr PacketMgr{ get; set; }

        public Action<AsyncUserToken> Closed { get; set; }
        public Action<AsyncUserToken> Connected { get; set; }

        public TCPSocketServer(int numConnections)
        {
            int max = ushort.MaxValue;
            if (numConnections < max)
            {
                NumConnections = numConnections;
                BufferSize = 1024 * 10; // IOCP接收数据缓存大小，设置过小会造成事件响应增多，设置过大会造成内存占用偏多

                PacketMgr = new PacketMgr();
                MaxNumberAccepted = new Semaphore(NumConnections, NumConnections);
                AsyncSocketUserTokenUsed = new List<AsyncUserToken>();
                AsyncSocketUserTokenPool = new ObjectPool<AsyncUserToken>(numConnections);

                AsyncUserToken socketToken;

                for (int i = 0; i < NumConnections; i++) //按照连接数建立读写对象
                {
                    socketToken = new AsyncUserToken(this, BufferSize);
                    socketToken.TokenID = i + 1;
                    socketToken.IsClient = false;
                    socketToken.PacketMgr = PacketMgr;
                    socketToken.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    socketToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    AsyncSocketUserTokenPool.Push(socketToken);
                }

                SocketTimeOutMS = 30 * 1000; // 30秒
                DaemonThread = new TCPSocketServerDaemonThread(); // 线程守护
                DaemonThread.TCPServer = this;
                DaemonThread.UserTokens = AsyncSocketUserTokenUsed;
                DaemonThread.Start();
            }
            else
            {
                throw new Exception(string.Format("链接数{0}超出最大连接数{1}", numConnections, max));
            }
        }

        public void Listen(string ip, int port)
        {
            IPEndPoint listenPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            try
            {
                ListenSocket = new Socket(listenPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(listenPoint);
                ListenSocket.Listen(NumConnections);
                App.Info("开启监听 {0} 成功", listenPoint.ToString());
                StartAccept(null);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("开启监听失败!{0}\n{1}", listenPoint, e));
            }
        }

        public void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                eventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }
            MaxNumberAccepted.WaitOne(); //获取信号量
            bool willRaiseEvent = ListenSocket.AcceptAsync(eventArgs);
            if (!willRaiseEvent) ProcessAccept(eventArgs);
        }

        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs eventArgs)
        {
            try
            {
                ProcessAccept(eventArgs);
            }
            catch (Exception e)
            {
                App.Error("接收客户端连接 {0} 错误, 消息: {1}", eventArgs.AcceptSocket, e.Message);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError == SocketError.Success)
            {
                App.Info("客户端已连接. 本地地址: {0}, 远程地址: {1}",
                    eventArgs.AcceptSocket.LocalEndPoint, eventArgs.AcceptSocket.RemoteEndPoint);

                AsyncUserToken socketToken = AsyncSocketUserTokenPool.Pop();
                AsyncSocketUserTokenUsed.Add(socketToken); //添加到正在连接列表
                socketToken.Socket = eventArgs.AcceptSocket;
                socketToken.ConnectDateTime = DateTime.Now;
                Connected?.Invoke(socketToken); // 连接回调
                try
                {
                    bool willRaiseEvent = socketToken.Socket.ReceiveAsync(socketToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent) OnRecvAsync(socketToken.RecvEventArgs);
                }
                catch (Exception e)
                {
                    App.Error("接收客户端连接 {0} 错误, 消息: {1}", socketToken.Socket, e.Message);
                }
            }
            StartAccept(eventArgs); //把当前异步事件释放，等待下次连接
        }

        protected void IO_Completed(object sender, SocketAsyncEventArgs eventArgs)
        {
            AsyncUserToken socketToken = eventArgs.UserToken as AsyncUserToken;
            socketToken.ActiveDateTime = DateTime.Now;
            try
            {
                if (socketToken.Socket != null && socketToken.Socket.Connected)
                {
                    if (eventArgs.LastOperation == SocketAsyncOperation.Send)
                        OnSendAsync(eventArgs);
                    else if (eventArgs.LastOperation == SocketAsyncOperation.Receive)
                        OnRecvAsync(eventArgs);
                    else
                    {
                        throw new ArgumentException(string.Format("操作错误, 当前Socket={0}最后执行的不是\"发送\"或\"接收\"操作", socketToken.TokenID));
                    }
                }
            }
            catch (Exception e)
            {
                App.Error("IO_Completed {0} {1} 错误, 消息: {2}", socketToken.Socket, eventArgs.LastOperation, e);
            }
        }

        protected void OnRecvAsync(SocketAsyncEventArgs eventArgs)
        {
            RecvAsync(eventArgs.UserToken as AsyncUserToken);
        }

        public virtual void RecvAsync(AsyncUserToken userToken)
        {
            userToken.ActiveDateTime = DateTime.Now;
            if (userToken.RecvEventArgs.BytesTransferred > 0 && 
                userToken.RecvEventArgs.SocketError == SocketError.Success)
            {
                //如果处理数据返回失败，则断开连接
                if (!userToken.RecvAsync()) CloseSocket(userToken);
                else //否则投递下次接收数据请求
                {
                    // willRaiseEvent 
                    // true  I/O操作处于挂起状态(会引发SocketAsyncEventArgs.Completed 上的事件e), 
                    // false I/O操作同步完成(不会引发会引发SocketAsyncEventArgs.Completed 上的事件e，可能是直接返回操作结果)
                    bool willRaiseEvent = userToken.Socket.ReceiveAsync(userToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent) OnRecvAsync(userToken.RecvEventArgs);
                }
            }
            else
            {
                CloseSocket(userToken);
            }
        }

        protected bool OnSendAsync(SocketAsyncEventArgs eventArgs)
        {
            AsyncUserToken socketToken = eventArgs.UserToken as AsyncUserToken;

            socketToken.ActiveDateTime = DateTime.Now;

            if (eventArgs.SocketError == SocketError.Success)
            {
                return socketToken.SendAsync(true); //调用子类回调函数
            }
            else
            {
                CloseSocket(socketToken);
                return false;
            }
        }

        public virtual bool SendAsync(AsyncUserToken userToken)
        {
            try
            {
                bool willRaiseEvent = userToken.Socket.SendAsync(userToken.SendEventArgs);

                if (willRaiseEvent)
                    return true;
                else
                {
                    return OnSendAsync(userToken.SendEventArgs);
                }
            }
            catch (Exception e)
            {
                App.Error("发送数据 {0} 错误, 消息: {1}", userToken.Socket, e.Message);
                return false;
            }
        }

        public virtual void CloseSocket(AsyncUserToken socketToken)
        {
            if (socketToken.Socket == null)
                return;
            string socketInfo = string.Format("本地地址: {0} 远程地址: {1}", socketToken.Socket.LocalEndPoint,
                socketToken.Socket.RemoteEndPoint);
            try
            {
                socketToken.Socket.Shutdown(SocketShutdown.Both);
                App.Info("关闭连接. {0}", socketInfo);
            }
            catch (Exception e)
            {
                App.Info("关闭连接 {0} 错误, 消息: {1}", socketInfo, e.Message);
            }
            socketToken.Socket.Close();
            socketToken.Socket = null; //释放引用，并清理缓存，包括释放协议对象等资源
            socketToken.Clear();

            AsyncSocketUserTokenUsed.Remove(socketToken);
            AsyncSocketUserTokenPool.Push(socketToken);

            MaxNumberAccepted.Release();

            Closed?.Invoke(socketToken); // 连接关闭回调
        }

        public void CloseAllSocket()
        {
            for (int i = 0; i < AsyncSocketUserTokenUsed.Count; i++)
            {
                AsyncUserToken socketToken = AsyncSocketUserTokenUsed[i];
                CloseSocket(socketToken);
            }
            App.Info("已经关闭所有连接.");
        }
    }
}
