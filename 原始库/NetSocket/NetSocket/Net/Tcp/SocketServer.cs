using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetSocket.Tcp
{
    public class SocketServer : AsyncSocket
    {
        public int SocketTimeOutMS { get; set; }

        private DaemonThread DaemonThread { get; set; }

        public SocketServer(int numConnections): base(numConnections)
        {
        }

        public void Start(string host, int port)
        {
            IPEndPoint listenPoint = new IPEndPoint(IPAddress.Parse(host), port);
            Start(listenPoint);
        }

        public void Start(IPEndPoint localEndPoint)
        {
            ListenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            ListenSocket.Bind(localEndPoint);
            ListenSocket.Listen(NumConnections);
            Console.WriteLine("Start listen socket {0} success", localEndPoint.ToString());
            StartAccept(null);
            SocketTimeOutMS = 60 * 1000;
            DaemonThread = new DaemonThread(this);
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArgs.AcceptSocket = null;
            }

            MaxNumberAccepted.WaitOne();
            bool willRaiseEvent = ListenSocket.AcceptAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }

        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception E)
            {
                Console.WriteLine("Accept client {0} error, message: {1}", acceptEventArgs.AcceptSocket, E.Message);
                Console.WriteLine(E.StackTrace);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            Console.WriteLine("Client connection accepted. Local Address: {0}, Remote Address: {1}",
                acceptEventArgs.AcceptSocket.LocalEndPoint, acceptEventArgs.AcceptSocket.RemoteEndPoint);

            AsyncSocketUserToken userToken = AsyncSocketUserTokenPool.Pop();
            AsyncSocketUserTokenList.Add(userToken);
            userToken.ConnectSocket = acceptEventArgs.AcceptSocket;
            userToken.ConnectDateTime = DateTime.Now;

            try
            {
                bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs);
                if (!willRaiseEvent)
                {
                    lock (userToken)
                    {
                        ProcessReceive(userToken.ReceiveEventArgs);
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Accept client {0} error, message: {1}", userToken.ConnectSocket, E.Message);
                Console.WriteLine(E.StackTrace);
            }

            StartAccept(acceptEventArgs);
        }
    }
}
