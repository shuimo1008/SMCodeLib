using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ZCSharpLib.Generics;
using System.Collections.Generic;

namespace ZCSharpLib.Networks.TCP
{
    public class TCPServer : IAsyncNet
    {
        protected Socket ListenSocket { get; set; }

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
        protected ObjectPool<AsyncUserToken> AsyncSocketUserTokenPool { get; set; }
        protected Dictionary<int, AsyncUserToken> AsyncSocketUserTokenUsed { get; set; }

        /// <summary>
        /// 数据包接收者
        /// </summary>
        public Action<Packet> Receiver { get; set; }

        /// <summary>
        /// 数据接收线程锁
        /// </summary>
        private object recvSync = new object();
        /// <summary>
        /// 数据发送线程锁
        /// </summary>
        private object sendSync = new object();

        public TCPServer(int numConnections)
        {
            int max = ushort.MaxValue;
            if (numConnections < max)
            {
                NumConnections = numConnections;
                BufferSize = 1024 * 10; // IOCP接收数据缓存大小，设置过小会造成事件响应增多，设置过大会造成内存占用偏多

                PacketFactory oPacketFactory = new PacketFactory();
                MaxNumberAccepted = new Semaphore(NumConnections, NumConnections);
                AsyncSocketUserTokenUsed = new Dictionary<int, AsyncUserToken>();
                AsyncSocketUserTokenPool = new ObjectPool<AsyncUserToken>(numConnections);

                AsyncUserToken oSocketToken;
                Guids.LongGuid oGuid = new Guids.LongGuid(0);
                for (int i = 0; i < NumConnections; i++) //按照连接数建立读写对象
                {
                    ushort oTokenID = (ushort)oGuid.GenerateGuid();
                    oSocketToken = new AsyncUserToken(this, BufferSize);
                    oSocketToken.OwnerID1 = oTokenID;
                    oSocketToken.PacketFactory = oPacketFactory;
                    oSocketToken.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    oSocketToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                    AsyncSocketUserTokenPool.Push(oSocketToken);
                }
            }
            else
            {
                throw new Exception(string.Format("链接数{0}超出最大连接数65535", numConnections));
            }
        }

        public void Listen(string host, int port)
        {
            IPEndPoint listenPoint = new IPEndPoint(IPAddress.Parse(host), port);
            ListenSocket = new Socket(listenPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ListenSocket.Bind(listenPoint);
            ListenSocket.Listen(NumConnections);
            App.Logger.Info("Start listen socket {0} success", listenPoint.ToString());
            StartAccept(null);
        }

        public void StartAccept(SocketAsyncEventArgs iSocketAsyncEventArgs)
        {
            if (iSocketAsyncEventArgs == null)
            {
                iSocketAsyncEventArgs = new SocketAsyncEventArgs();
                iSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                iSocketAsyncEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }
            MaxNumberAccepted.WaitOne(); //获取信号量
            bool willRaiseEvent = ListenSocket.AcceptAsync(iSocketAsyncEventArgs);
            if (!willRaiseEvent) ProcessAccept(iSocketAsyncEventArgs);
        }

        void AcceptEventArg_Completed(object iSender, SocketAsyncEventArgs iSocketAsyncEventArgs)
        {
            try
            {
                ProcessAccept(iSocketAsyncEventArgs);
            }
            catch (Exception e)
            {
                App.Logger.Error("Accept client {0} error, message: {1}", iSocketAsyncEventArgs.AcceptSocket, e.Message);
                App.Logger.Error(e.StackTrace);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs iSocketAsyncEventArgs)
        {
            if (iSocketAsyncEventArgs.SocketError == SocketError.Success)
            {
                App.Logger.Info("Client connection accepted. Local Address: {0}, Remote Address: {1}",
                    iSocketAsyncEventArgs.AcceptSocket.LocalEndPoint, iSocketAsyncEventArgs.AcceptSocket.RemoteEndPoint);

                AsyncUserToken oSocketToken = AsyncSocketUserTokenPool.Pop();
                AsyncSocketUserTokenUsed.Add(oSocketToken.OwnerID1, oSocketToken); //添加到正在连接列表
                oSocketToken.Socket = iSocketAsyncEventArgs.AcceptSocket;
                oSocketToken.ConnectDateTime = DateTime.Now;
                try
                {
                    bool willRaiseEvent = oSocketToken.Socket.ReceiveAsync(oSocketToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent) OnRecvAsync(oSocketToken.RecvEventArgs);
                }
                catch (Exception e)
                {
                    App.Logger.Error("Accept client {0} error, message: {1}", oSocketToken.Socket, e.Message);
                    App.Logger.Error(e.StackTrace);
                }
            }
            StartAccept(iSocketAsyncEventArgs); //把当前异步事件释放，等待下次连接
        }

        protected void IO_Completed(object iSender, SocketAsyncEventArgs iEventArgs)
        {
            AsyncUserToken oSocketToken = iEventArgs.UserToken as AsyncUserToken;
            oSocketToken.ActiveDateTime = DateTime.Now;
            try
            {
                if (iEventArgs.LastOperation == SocketAsyncOperation.Receive)
                    lock (recvSync)
                    {
                        OnRecvAsync(iEventArgs);
                    }
                else if (iEventArgs.LastOperation == SocketAsyncOperation.Send)
                    lock (sendSync)
                    {
                        OnSendAsync(iEventArgs);
                    }
                else
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
            catch (Exception e)
            {
                App.Logger.Error("IO_Completed {0} error, message: {1}", oSocketToken.Socket, e.Message);
                App.Logger.Error(e.StackTrace);
            }
        }

        protected void OnRecvAsync(SocketAsyncEventArgs iRecvEventArgs)
        {
            RecvAsync(iRecvEventArgs.UserToken as AsyncUserToken);
        }

        public virtual void RecvAsync(AsyncUserToken iAsyncUserToken)
        {
            iAsyncUserToken.ActiveDateTime = DateTime.Now;
            if (iAsyncUserToken.RecvEventArgs.BytesTransferred > 0 && 
                iAsyncUserToken.RecvEventArgs.SocketError == SocketError.Success)
            {
                //如果处理数据返回失败，则断开连接
                if (!iAsyncUserToken.RecvAsync()) CloseSocket(iAsyncUserToken);
                else //否则投递下次接收数据请求
                {
                    // willRaiseEvent 
                    // true I/O操作处于挂起状态(会引发SocketAsyncEventArgs.Completed 上的事件e), 
                    // false I/O操作同步完成(不会引发会引发SocketAsyncEventArgs.Completed 上的事件e，可能是直接返回操作结果)
                    bool willRaiseEvent = iAsyncUserToken.Socket.ReceiveAsync(iAsyncUserToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent) OnRecvAsync(iAsyncUserToken.RecvEventArgs);
                }
            }
            else
            {
                CloseSocket(iAsyncUserToken);
            }
        }

        protected bool OnSendAsync(SocketAsyncEventArgs iSocketAsyncEventArgs)
        {
            AsyncUserToken oSocketToken = iSocketAsyncEventArgs.UserToken as AsyncUserToken;

            oSocketToken.ActiveDateTime = DateTime.Now;

            if (iSocketAsyncEventArgs.SocketError == SocketError.Success)
            {
                return oSocketToken.SendAsyncCompleted(); //调用子类回调函数
            }
            else
            {
                CloseSocket(oSocketToken);
                return false;
            }
        }

        public virtual bool SendAsync(AsyncUserToken iSocketToken)
        {
            bool willRaiseEvent = iSocketToken.Socket.SendAsync(iSocketToken.SendEventArgs);

            if (!willRaiseEvent)
            {
                return OnSendAsync(iSocketToken.SendEventArgs);
            }
            else
            {
                return true;
            }
        }

        public virtual void CloseSocket(AsyncUserToken iSocketToken)
        {
            if (iSocketToken.Socket == null)
                return;
            string socketInfo = string.Format("Local Address: {0} Remote Address: {1}", iSocketToken.Socket.LocalEndPoint,
                iSocketToken.SendEventArgs.RemoteEndPoint);
            App.Logger.Info("Connection disconnected. {0}", socketInfo);
            try
            {
                iSocketToken.Socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                App.Logger.Info("CloseSocket Disconnect {0} error, message: {1}", socketInfo, e.Message);
            }
            iSocketToken.Socket.Close();
            iSocketToken.Socket = null; //释放引用，并清理缓存，包括释放协议对象等资源
            iSocketToken.Clear();
            AsyncSocketUserTokenUsed.Remove(iSocketToken.OwnerID1);
            AsyncSocketUserTokenPool.Push(iSocketToken);

            MaxNumberAccepted.Release();
        }

        public void CloseSocketAll()
        {
            AsyncUserToken[] oUserTokens = null;
            AsyncSocketUserTokenUsed.Values.CopyTo(oUserTokens, 0);
            for (int i = 0; i < oUserTokens.Length; i++)
            {
                AsyncUserToken oUserToken = oUserTokens[i];
                CloseSocket(oUserToken);
            }
        }

        #region 接收/发送操作
        public void Send(Packet iPacket)
        {
            lock (sendSync)
            {
                AsyncUserToken oAsyncUserToken = null;
                if (AsyncSocketUserTokenUsed.TryGetValue(iPacket.OwnerID1, out oAsyncUserToken))
                {
                    ByteArray oByteArray = new ByteArray();
                    iPacket.Serialization(oByteArray, true);
                    oAsyncUserToken.SendAsync(oByteArray.Bytes);
                }
            }
        }

        public void Recive(Packet iPacket)
        {
            lock (recvSync)
            {
               Receiver?.Invoke(iPacket);
            }
        }
        #endregion
    }
}
