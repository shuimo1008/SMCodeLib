using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ZCSharpLib.Events;

namespace ZCSharpLib.Networks.TCP
{
    public class TCPClient : IAsyncNet
    {
        /// <summary>
        /// 是否链接
        /// </summary>
        public bool Connected
        {
            get
            {
                return ConnectSocket.Connected;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected Socket ConnectSocket { get; set; }
        /// <summary>
        /// 每个连接接收缓存大小
        /// </summary>
        protected int BufferSize { get; set; }
        /// <summary>
        /// 包处理
        /// </summary>
        public Action<Packet> Receiver { get; set; }
        public AsyncUserToken AsyncSocketToken { get; protected set; }

        /// <summary>
        /// 接收同步锁
        /// </summary>
        private object recvSync = new object();
        /// <summary>
        /// 发送同步锁
        /// </summary>
        private object sendSync = new object();

        public TCPClient(ushort clientID = 0)
        {
            BufferSize = 1024 * 1024;
            PacketFactory oPacketFactory = new PacketFactory();
            AsyncSocketToken = new AsyncUserToken(this, BufferSize);
            AsyncSocketToken.OwnerID2 = clientID;
            AsyncSocketToken.PacketFactory = oPacketFactory;
            AsyncSocketToken.RecvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            AsyncSocketToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void Connect(string host, int port)
        {
            IPEndPoint oIPEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            ConnectSocket = new Socket(oIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs oConnectEventArgs = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = oIPEndPoint,
                AcceptSocket = ConnectSocket
            };
            oConnectEventArgs.Completed += ConnectEventArg_Completed;
            StartConnect(oConnectEventArgs);
        }

        private void StartConnect(SocketAsyncEventArgs iSocketAsyncEventArgs)
        {
            bool willRaiseEvent = ConnectSocket.ConnectAsync(iSocketAsyncEventArgs);
            if (!willRaiseEvent)
            {
                ProcessConnect(iSocketAsyncEventArgs);
            }
        }

        private void ConnectEventArg_Completed(object iSender, SocketAsyncEventArgs iSocketAsyncEventArgs)
        {
            try
            {
                ProcessConnect(iSocketAsyncEventArgs);
            }
            catch (Exception e)
            {
                App.Logger.Error("Accept client {0} error, message: {1}", iSocketAsyncEventArgs.AcceptSocket, e.Message);
                App.Logger.Error(e.StackTrace);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs iSocketAsyncEventArgs)
        {
            AsyncSocketToken.ConnectDateTime = DateTime.Now;
            AsyncSocketToken.Socket = iSocketAsyncEventArgs.AcceptSocket;

            if (iSocketAsyncEventArgs.SocketError == SocketError.Success)
            {
                try
                {
                    bool willRaiseEvent = AsyncSocketToken.Socket.ReceiveAsync(AsyncSocketToken.RecvEventArgs); //投递接收请求
                    if (!willRaiseEvent)
                    {
                        lock (AsyncSocketToken)
                        {
                            // TODO 处理接收到的消息
                            OnRecvAsync(AsyncSocketToken.RecvEventArgs);
                        }
                    }
                }
                catch (Exception e)
                {
                    App.Logger.Error("Accept client {0} error, message: {1}", AsyncSocketToken.Socket, e.Message);
                    App.Logger.Error(e.StackTrace);
                }
            }
            else
            {
                CloseSocket(AsyncSocketToken);
            }
        }

        protected void IO_Completed(object iSender, SocketAsyncEventArgs iEventArgs)
        {
            lock (recvSync)
            {
                AsyncUserToken oSocketToken = iEventArgs.UserToken as AsyncUserToken;
                oSocketToken.ActiveDateTime = DateTime.Now;
                try
                {
                    if (iEventArgs.LastOperation == SocketAsyncOperation.Receive)
                        lock(recvSync) OnRecvAsync(iEventArgs);
                    else if (iEventArgs.LastOperation == SocketAsyncOperation.Send)
                        lock(sendSync) OnSendAsync(iEventArgs);
                    else
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
                catch (Exception e)
                {
                    App.Logger.Error("IO_Completed {0} error, message: {1}", oSocketToken.Socket, e.Message);
                    App.Logger.Error(e.StackTrace);
                }
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
                else
                {
                    //否则投递下次接收数据请求
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
        }

        #region 接收/发送操作
        public void Send(Packet iPacket)
        {
            lock (sendSync)
            {
                ByteArray oByteArray = new ByteArray();
                iPacket.OwnerID2 = AsyncSocketToken.OwnerID2;
                iPacket.Serialization(oByteArray, true);
                AsyncSocketToken.SendAsync(oByteArray.Bytes);
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
