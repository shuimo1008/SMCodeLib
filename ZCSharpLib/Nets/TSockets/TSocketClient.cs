using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ZCSharpLib.Nets.TSockets
{
    public class TSocketClient : IAsyncScoket
    {
        protected Socket ConnectSocket { get; set; }
        public bool IsConnected => ConnectSocket.Connected;
        /// <summary>
        /// 每个连接接收缓存大小
        /// </summary>
        protected int BufferSize { get; set; }
        public AsyncUserToken UserToken { get; protected set; }

        public Action<NetworkStatus, AsyncUserToken> OnConnectEvent { get; set; }

        public TSocketClient()
        {
            BufferSize = 1024 * 1024 * 10;
            UserToken = new AsyncUserToken(this, BufferSize);
            UserToken.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            UserToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public TSocketClient BindStream<T>()
            where T : class, IDataStream
        {
            UserToken.Bind<T>();
            return GetSelf();
        }
        public TSocketClient GetSelf() => this;

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
            App.Info($"连接 {ipEndPoint}");
        }

        private void StartConnect(SocketAsyncEventArgs eventArgs)
        {
            OnConnectEvent?.Invoke(NetworkStatus.Connecting, UserToken);
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
                App.Error($"接收客户端连接 {eventArgs.AcceptSocket} 错误, 消息: {e.Message}");
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs eventArgs)
        {
            UserToken.Socket = eventArgs.AcceptSocket;
            UserToken.ActiveDateTime = DateTime.Now;

            if (eventArgs.SocketError == SocketError.Success)
            {

                App.Info($"服务器已连接. 本地地址: {eventArgs.AcceptSocket.LocalEndPoint}, 远程地址: {eventArgs.RemoteEndPoint}");

                OnConnectEvent?.Invoke(NetworkStatus.Connected, UserToken);

                try
                {
                    bool willRaiseEvent = UserToken.Socket.ReceiveAsync(UserToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent) OnRecvAsync(UserToken.RecvEventArgs);// 处理接收到的消息
                }
                catch (Exception e)
                {
                    App.Error($"接收客户端连接 {UserToken.Socket} 错误, 消息: {e.Message}");
                }
            }
            else
            {
                CloseSocket(UserToken);
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
                        throw new ArgumentException($"操作错误, 当前Socket={socketToken.SessionID}最后执行的不是\"发送\"或\"接收\"操作");
                }
            }
            catch (Exception e)
            {
                App.Error($"IO_Completed {socketToken.Socket} {eventArgs.LastOperation} 错误, 消息: {e}");
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
            AsyncUserToken userToken = socketAsyncEventArgs.UserToken as AsyncUserToken;

            userToken.ActiveDateTime = DateTime.Now;

            if (socketAsyncEventArgs.SocketError == SocketError.Success)
            {
                return userToken.SendAsync(true); //调用子类回调函数
            }
            else
            {
                CloseSocket(userToken);
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
                    return OnSendAsync(userToken.SendEventArgs);
            }
            catch (Exception e)
            {
                App.Error($"发送数据 {userToken.Socket} 错误, 消息: {e.Message}");
                return false;
            }
        }

        public int Send(AsyncUserToken userToken, byte[] buffer, int offset, int count)
        {
            return userToken.Socket.Send(buffer, offset, count, SocketFlags.None);
        }

        public virtual void CloseSocket(AsyncUserToken userToken)
        {
            if (userToken.Socket == null)
                return;
            string socketInfo = string.Format("本地地址:{0}", userToken.Socket.LocalEndPoint);
            try
            {
                userToken.Socket.Shutdown(SocketShutdown.Both);
                App.Info($"关闭连接. {socketInfo}");
            }
            catch (Exception e)
            {
                App.Info($"关闭连接 {socketInfo} 错误, 消息: {e.Message}");
            }
            userToken.Socket.Close();
            userToken.Socket = null; //释放引用，并清理缓存，包括释放协议对象等资源
            userToken.Clear();

            OnConnectEvent?.Invoke(NetworkStatus.Disconnect, UserToken);
        }
    }
}
