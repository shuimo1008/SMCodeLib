using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetSocket.Tcp
{
    public class SocketClient : AsyncSocket
    {
        public AsyncSocketUserToken UserToken { get; private set; }

        public SocketClient() : base(1)
        {
            UserToken = AsyncSocketUserTokenPool.Pop();
            AsyncSocketUserTokenList.Add(UserToken);
        }

        public void Connect(string host, int port)
        {
            IPEndPoint oIPEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            ListenSocket = new Socket(oIPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs connectEventArgs = new SocketAsyncEventArgs() { RemoteEndPoint = oIPEndPoint, AcceptSocket = ListenSocket };
            connectEventArgs.Completed += ConnectEventArg_Completed;

            MaxNumberAccepted.WaitOne();
            StartConnect(connectEventArgs);
        }

        private void StartConnect(SocketAsyncEventArgs connectEventArgs)
        {
            bool willRaiseEvent = ListenSocket.ConnectAsync(connectEventArgs);
            if (!willRaiseEvent)
            {
                ProcessConnect(connectEventArgs);
            }
        }

        private void ConnectEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessConnect(e);
            }
            catch (Exception E)
            {
                Console.WriteLine("Accept client {0} error, message: {1}", e.AcceptSocket, E.Message);
                Console.WriteLine(E.StackTrace);
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError == SocketError.SocketError) return;

            UserToken.ConnectDateTime = DateTime.Now;

            UserToken.ConnectSocket = acceptEventArgs.AcceptSocket;

            try
            {
                bool willRaiseEvent = UserToken.ConnectSocket.ReceiveAsync(UserToken.ReceiveEventArgs);
                if (!willRaiseEvent)
                {
                    lock (UserToken)
                    {
                        ProcessReceive(UserToken.ReceiveEventArgs);
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Accept client {0} error, message: {1}", UserToken.ConnectSocket, E.Message);
                Console.WriteLine(E.StackTrace);
            }
        }
    }
}
