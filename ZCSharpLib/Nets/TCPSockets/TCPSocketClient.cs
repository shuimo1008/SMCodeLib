using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ZCSharpLib.Nets.TCPSockets
{
    public class TCPSocketClient : IAsyncScoket
    {
        /// <summary>
        /// 是否链接
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return ConnectSocket.Connected;
            }
        }
        protected Socket ConnectSocket { get; set; }
        /// <summary>
        /// 每个连接接收缓存大小
        /// </summary>
        protected int BufferSize { get; set; }
        /// <summary>
        /// 包处理
        /// </summary>
        public PacketMgr PacketMgr { get; set; }
        public AsyncUserToken SocketToken { get; protected set; }

        public Action<AsyncUserToken> Closed { get; set; }
        public Action<AsyncUserToken> Connected { get; set; }

        public TCPSocketClient()
        {
            BufferSize = 1024 * 1024 * 10;
            PacketMgr = new PacketMgr();
            SocketToken = new AsyncUserToken(this, BufferSize);
            SocketToken.IsClient = true;
            SocketToken.PacketMgr = PacketMgr;
            SocketToken.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            SocketToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void Connect(string ip, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            ConnectSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs oConnectEventArgs = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = ipEndPoint,
                AcceptSocket = ConnectSocket
            };
            oConnectEventArgs.Completed += ConnectEventArg_Completed;
            StartConnect(oConnectEventArgs);
            App.Info("连接 {0}", ipEndPoint.ToString());
        }

        private void StartConnect(SocketAsyncEventArgs eventArgs)
        {
            bool willRaiseEvent = ConnectSocket.ConnectAsync(eventArgs);
            if (!willRaiseEvent) ProcessConnect(eventArgs);
        }

        private void ConnectEventArg_Completed(object sender, SocketAsyncEventArgs eventArgs)
        {
            try
            {
                ProcessConnect(eventArgs);
            }
            catch (Exception e)
            {
                App.Error("接收客户端连接 {0} 错误, 消息: {1}", eventArgs.AcceptSocket, e.Message);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs eventArgs)
        {
            SocketToken.ConnectDateTime = DateTime.Now;
            SocketToken.Socket = eventArgs.AcceptSocket;

            if (eventArgs.SocketError == SocketError.Success)
            {

                App.Info("服务器已连接. 本地地址: {0}, 远程地址: {1}",
                   eventArgs.AcceptSocket.LocalEndPoint, eventArgs.RemoteEndPoint);

                Connected?.Invoke(SocketToken);

                try
                {
                    bool willRaiseEvent = SocketToken.Socket.ReceiveAsync(SocketToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent)
                    {
                        lock (SocketToken)
                        {
                            // TODO 处理接收到的消息
                            OnRecvAsync(SocketToken.RecvEventArgs);
                        }
                    }
                }
                catch (Exception e)
                {
                    App.Error("接收客户端连接 {0} 错误, 消息: {1}", SocketToken.Socket, e.Message);
                }
            }
            else
            {
                CloseSocket(SocketToken);
            }
        }

        protected void IO_Completed(object sender, SocketAsyncEventArgs eventArgs)
        {
            AsyncUserToken socketToken = eventArgs.UserToken as AsyncUserToken;
            socketToken.ActiveDateTime = DateTime.Now;
            try
            {
                if (socketToken.Socket != null && socketToken.Socket.Connected)
                {
                    if (eventArgs.LastOperation == SocketAsyncOperation.Receive)
                        OnRecvAsync(eventArgs);
                    else if (eventArgs.LastOperation == SocketAsyncOperation.Send)
                        OnSendAsync(eventArgs);
                    else
                        throw new ArgumentException(string.Format("操作错误, 当前Socket={0}最后执行的不是\"发送\"或\"接收\"操作", socketToken.TokenID));
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

        public virtual void RecvAsync(AsyncUserToken asyncUserToken)
        {
            asyncUserToken.ActiveDateTime = DateTime.Now;
            if (asyncUserToken.RecvEventArgs.BytesTransferred > 0 && 
                asyncUserToken.RecvEventArgs.SocketError == SocketError.Success)
            {
                //如果处理数据返回失败，则断开连接
                if (!asyncUserToken.RecvAsync()) CloseSocket(asyncUserToken);
                else
                {
                    //否则投递下次接收数据请求
                    // true I/O操作处于挂起状态(会引发SocketAsyncEventArgs.Completed 上的事件e), 
                    // false I/O操作同步完成(不会引发会引发SocketAsyncEventArgs.Completed 上的事件e，可能是直接返回操作结果)
                    bool willRaiseEvent = asyncUserToken.Socket.ReceiveAsync(asyncUserToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent) OnRecvAsync(asyncUserToken.RecvEventArgs);
                }
            }
            else
            {
                CloseSocket(asyncUserToken);
            }
        }

        protected bool OnSendAsync(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            AsyncUserToken socketToken = socketAsyncEventArgs.UserToken as AsyncUserToken;

            socketToken.ActiveDateTime = DateTime.Now;

            if (socketAsyncEventArgs.SocketError == SocketError.Success)
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

        public int Send(AsyncUserToken userToken, byte[] buffer, int offset, int count)
        {
            return userToken.Socket.Send(buffer, offset, count, SocketFlags.None);
        }

        public virtual void CloseSocket(AsyncUserToken socketToken)
        {
            if (socketToken.Socket == null)
                return;
            string socketInfo = string.Format("本地地址:{0}", socketToken.Socket.LocalEndPoint);
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

            Closed?.Invoke(socketToken);
        }
    }
}
