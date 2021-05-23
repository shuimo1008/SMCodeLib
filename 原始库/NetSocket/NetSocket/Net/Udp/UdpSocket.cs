using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetSocket.Udp
{
    public class UdpSocket : IAsyncSocket
    {
        protected Socket ListenSocket { get; set; }

        protected int ReceiveBufferSize { get; set; }

        public AsyncSocketUserToken UserToken { get; set; }

        public ProtocolMgr ProtocolMgr { get; set; }

        private bool IsServer { get; set; }

        private DaemonThread DaemonThread { get; set; }

        public Action OnClosed { get; set; }

        public UdpSocket()
        {
            DaemonThread = new DaemonThread();
            ProtocolMgr = new ProtocolMgr();
            ReceiveBufferSize = 1024;
            UserToken = new AsyncSocketUserToken(ReceiveBufferSize);
            UserToken.AsyncSocket = this;
            UserToken.ProtocolMgr = ProtocolMgr;
            UserToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            UserToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
        }

        public void Start(int port)
        {
            IPEndPoint listenPort = new IPEndPoint(IPAddress.Any, port);
            ListenSocket = new Socket(listenPort.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            //ListenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            ListenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            ListenSocket.EnableBroadcast = true;
            ListenSocket.Bind(listenPort);
            UserToken.ConnectSocket = ListenSocket;
            UserToken.ReceiveEventArgs.RemoteEndPoint = listenPort;
            UserToken.SendEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
            UserToken.ConnectSocket.ReceiveFromAsync(UserToken.ReceiveEventArgs);
            Console.WriteLine("Start listen socket {0} success", listenPort.ToString());
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            AsyncSocketUserToken userToken = e.UserToken as AsyncSocketUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            try
            {
                lock (userToken)
                {
                    if (e.LastOperation == SocketAsyncOperation.ReceiveFrom)
                        ProcessReceive(e);
                    else if (e.LastOperation == SocketAsyncOperation.SendTo)
                        ProcessSend(e);
                    else
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("IO_Completed {0} error, message: {1}", userToken.ConnectSocket, E.Message);
                Console.WriteLine(E.StackTrace);
            }
        }

        public void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            AsyncSocketUserToken userToken = receiveEventArgs.UserToken as AsyncSocketUserToken;
            if (userToken.ConnectSocket == null)
                return;

            lock (userToken)
            {
                userToken.ActiveDateTime = DateTime.Now;
                if (userToken.ReceiveEventArgs.BytesTransferred > 0 && userToken.ReceiveEventArgs.SocketError == SocketError.Success)
                {
                    int offset = userToken.ReceiveEventArgs.Offset;
                    int count = userToken.ReceiveEventArgs.BytesTransferred;

                    if (count > 0)
                    {
                        if (!userToken.Receive(userToken.ReceiveEventArgs.Buffer, offset, count))
                        {
                            CloseSocket();
                        }
                        else
                        {
                            bool willRaiseEvent = userToken.ConnectSocket.ReceiveFromAsync(userToken.ReceiveEventArgs);
                            if (!willRaiseEvent)
                                ProcessReceive(userToken.ReceiveEventArgs);
                        }
                    }
                    else
                    {
                        bool willRaiseEvent = userToken.ConnectSocket.ReceiveFromAsync(userToken.ReceiveEventArgs);
                        if (!willRaiseEvent)
                            ProcessReceive(userToken.ReceiveEventArgs);
                    }

                }
                else
                {
                    CloseSocket();
                }
            }
        }

        private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            AsyncSocketUserToken userToken = sendEventArgs.UserToken as AsyncSocketUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            if (sendEventArgs.SocketError == SocketError.Success)
                return userToken.SendAsyncCompleted();
            else
            {
                CloseSocket();
                return false;
            }
        }

        public bool SendAsync(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count)
        {
            if (connectSocket == null) { return false; }

            sendEventArgs.SetBuffer(buffer, offset, count);

            bool willRaiseEvent = connectSocket.SendToAsync(sendEventArgs);

            if (!willRaiseEvent)
            {
                return ProcessSend(sendEventArgs);
            }
            else
            {
                return true;
            }
        }

        public void CloseSocket()
        {
            try
            {
                ListenSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception E)
            {
                Console.WriteLine ("CloseSocket Disconnect error, message: {0}", E.Message);
            }
            ListenSocket.Close();
            ListenSocket = null;
            UserToken.TokenClose();
            OnClosed?.Invoke();
        }
    }
}
