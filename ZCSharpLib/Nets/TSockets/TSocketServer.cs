using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using ZCSharpLib.Cores;

namespace ZCSharpLib.Nets.TSockets
{
    public class TSocketServer : IAsyncScoket
    {
        /// <summary>
        /// 超时连接(默认1000毫秒=1分钟)
        /// </summary>
        public int SocketTimeOutNS { get; set; }
        /// <summary>
        /// Socket连接
        /// </summary>
        protected Socket ListenSocket { get; set; }
        /// <summary>
        /// 超时守护线程
        /// </summary>
        protected TSocketServerDaemonThread DaemonThread { get; set; }
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
        protected ObjectList<AsyncUserToken> AsyncSocketUserTokenUsed { get; set; }
        protected ObjectPool<AsyncUserToken> AsyncSocketUserTokenPool { get; set; }

        /// <summary>
        /// 包管理
        /// </summary>
        public PacketMgr PacketMgr{ get; set; }
        public int NumberOfConnections => AsyncSocketUserTokenUsed.Count;
        public Action<NetworkStatus, AsyncUserToken> ConnectStatus { get; set; }

        public TSocketServer(int numConnections)
        {
            int max = ushort.MaxValue;
            if (numConnections < max)
            {
                NumConnections = numConnections;
                BufferSize = 1024 * 10; // IOCP接收数据缓存大小，设置过小会造成事件响应增多，设置过大会造成内存占用偏多

                PacketMgr = new PacketMgr();
                MaxNumberAccepted = new Semaphore(NumConnections, NumConnections);
                AsyncSocketUserTokenUsed = new ObjectList<AsyncUserToken>();
                AsyncSocketUserTokenPool = new ObjectPool<AsyncUserToken>(numConnections);

                AsyncUserToken socketToken;

                for (int i = 0; i < NumConnections; i++) //按照连接数建立读写对象
                {
                    socketToken = new AsyncUserToken(this, BufferSize);
                    socketToken.SessionID = i + 1;
                    socketToken.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    socketToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    AsyncSocketUserTokenPool.Push(socketToken);
                }

                SocketTimeOutNS = 30 * 1000 * 10000; // 30秒
                DaemonThread = new TSocketServerDaemonThread(); // 线程守护
                DaemonThread.TCPServer = this;
                DaemonThread.UserTokens = AsyncSocketUserTokenUsed;
                DaemonThread.Start();
            }
            else
            {
                throw new Exception(string.Format("链接数{0}超出最大连接数{1}", numConnections, max));
            }
        }

        public TSocketServer BindStream<T>() 
            where T :class, IDataStream
        {
            foreach (var userToken in AsyncSocketUserTokenPool)
                ( userToken as AsyncUserToken).Bind<T>();
            return GetSelf();
        }

        public TSocketServer GetSelf() => this;

        public void Listen(string ip, int port)
        {
            IPEndPoint listenPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            try
            {
                ListenSocket = new Socket(listenPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(listenPoint);
                ListenSocket.Listen(NumConnections);
                App.Info($"开启监听 {listenPoint} 成功");
                StartAccept(null);
            }
            catch (Exception e)
            {
                throw new Exception($"开启监听失败!{listenPoint}\n{e}");
            }
        }

        private int semaphoreNum = 0;
        private object semaphoreSync = new object();
        public void StartAccept(SocketAsyncEventArgs eventArgs)
        {
            lock (semaphoreSync)
            {
                // 如果监听AcceptScoket已经关闭则返回
                if (ListenSocket == null) 
                    return;

                if (eventArgs == null)
                {
                    eventArgs = new SocketAsyncEventArgs();
                    eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
                }
                else eventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接

                MaxNumberAccepted.WaitOne(); //获取信号量
                Interlocked.Increment(ref semaphoreNum);
                bool willRaiseEvent = ListenSocket.AcceptAsync(eventArgs);
                if (!willRaiseEvent) ProcessAccept(eventArgs);
            }
        }


        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs eventArgs)
        {
            try
            {
                ProcessAccept(eventArgs);
            }
            catch (Exception e)
            {
                App.Error($"接收客户端连接 {eventArgs.AcceptSocket} 错误, 消息: {e.Message}");
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError == SocketError.Success)
            {
                App.Info($"客户端已连接. 本地地址: {eventArgs.AcceptSocket.LocalEndPoint}, 远程地址: {eventArgs.AcceptSocket.RemoteEndPoint}");

                AsyncUserToken userToken = AsyncSocketUserTokenPool.Pop();
                AsyncSocketUserTokenUsed.Add(userToken);    //添加到正在连接列表
                userToken.Socket = eventArgs.AcceptSocket;
                userToken.ActiveDateTime = DateTime.Now;
                ConnectStatus?.Invoke(NetworkStatus.Connected, userToken); // 连接回调
                try
                {
                    bool willRaiseEvent = userToken.Socket.ReceiveAsync(userToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent) OnRecvAsync(userToken.RecvEventArgs);
                }
                catch (Exception e)
                {
                    App.Error($"接收客户端连接 {userToken.Socket} 错误, 消息: {e.Message}");
                }
            }
            StartAccept(eventArgs); //把当前异步事件释放，等待下次连接
        }

        protected void IO_Completed(object sender, SocketAsyncEventArgs eventArgs)
        {
            AsyncUserToken userToken = eventArgs.UserToken as AsyncUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            try
            {
                if (userToken.Socket != null && userToken.Socket.Connected)
                {
                    if (eventArgs.LastOperation == SocketAsyncOperation.Send)
                        OnSendAsync(eventArgs);
                    else if (eventArgs.LastOperation == SocketAsyncOperation.Receive)
                        OnRecvAsync(eventArgs);
                    else throw new ArgumentException($"操作错误, 当前Socket={userToken.SessionID}最后执行的不是\"发送\"或\"接收\"操作");
                }
            }
            catch (Exception e)
            {
                App.Error($"IO_Completed {userToken.Socket} {eventArgs.LastOperation} 错误, 消息: {e}");
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
                App.Error($"RecvAsync 字节或连接异常!{userToken.RecvEventArgs.BytesTransferred},{userToken.RecvEventArgs.SocketError}");
                CloseSocket(userToken);
            }
        }

        protected bool OnSendAsync(SocketAsyncEventArgs eventArgs)
        {
            AsyncUserToken userToken = eventArgs.UserToken as AsyncUserToken;

            userToken.ActiveDateTime = DateTime.Now;

            if (eventArgs.SocketError == SocketError.Success)
                return userToken.SendAsync(true); //调用子类回调函数
            else
            {
                App.Error($"SendAsync 连接异常!{userToken.RecvEventArgs.BytesTransferred},{userToken.RecvEventArgs.SocketError}");
                CloseSocket(userToken);
            }
            return false;
        }

        public virtual bool SendAsync(AsyncUserToken userToken)
        {
            try
            {
                bool willRaiseEvent = userToken.Socket.SendAsync(userToken.SendEventArgs);
                if (!willRaiseEvent)
                    return OnSendAsync(userToken.SendEventArgs);
                return willRaiseEvent;
            }
            catch (Exception e){
                App.Error($"发送数据 {userToken.Socket} 错误, 消息: {e.Message}");
            }
            return false;
        }

        public virtual void CloseSocket(AsyncUserToken userToken)
        {
            string socketInfo = string.Format($"本地地址: {userToken.Socket.LocalEndPoint} 远程地址: {userToken.Socket.RemoteEndPoint}");
            try
            {
                userToken.Socket.Shutdown(SocketShutdown.Both);
                App.Info($"关闭连接. {socketInfo}");
            }
            catch (Exception e)
            {
                App.Error($"关闭连接 {socketInfo} 错误, 消息: {e.Message}");
            }

            userToken.Socket.Close();
            userToken.Socket = null; //释放引用，并清理缓存，包括释放协议对象等资源
            userToken.Clear();

            AsyncSocketUserTokenUsed.Remove(userToken);
            AsyncSocketUserTokenPool.Push(userToken);

            MaxNumberAccepted.Release();// 信号量释放
            Interlocked.Decrement(ref semaphoreNum); 
            // 连接关闭回调
            ConnectStatus?.Invoke(NetworkStatus.Disconnect, userToken);
        }

        public void CloseSocketAll()
        {
            // 关闭所有连接,List.Remove会自动位移元素,
            // 所以这里需要另存一个数组对象用于关闭连接.
            int count = AsyncSocketUserTokenUsed.Count;
            AsyncUserToken[] userTokens = new AsyncUserToken[count];
            for (int i = 0; i < count; i++)
                userTokens[i] = AsyncSocketUserTokenUsed[i];
            for (int i = 0; i < count; i++) CloseSocket(userTokens[i]);

            lock (semaphoreSync)
            {
                if (ListenSocket != null)
                {
                    ListenSocket.Close();
                    ListenSocket = null;
                }
            }
            if (semaphoreNum > 0)
            {
                MaxNumberAccepted.Release(semaphoreNum);
                Interlocked.Add(ref semaphoreNum, -semaphoreNum);
            }
            App.Info("已经关闭所有连接.");
        }
    }
}
