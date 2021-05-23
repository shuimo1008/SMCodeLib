using NetSocket.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NetSocket.Tcp
{
    public abstract class AsyncSocket : IAsyncSocket
    {
        protected Socket ListenSocket { get; set; }

        protected int ReceiveBufferSize { get; set; }

        protected int NumConnections { get; set; }

        protected Semaphore MaxNumberAccepted { get; set; }

        public ObjectPool<AsyncSocketUserToken> AsyncSocketUserTokenPool { get; protected set; }
        public ObjectList<AsyncSocketUserToken> AsyncSocketUserTokenList { get; protected set; }

        protected ProtocolType ProtocolType { get; set; }

        public AsyncSocket(int numConnections)
        {
            NumConnections = numConnections;
            ReceiveBufferSize = 1024;
            AsyncSocketUserTokenPool = new ObjectPool<AsyncSocketUserToken>(numConnections);
            AsyncSocketUserTokenList = new ObjectList<AsyncSocketUserToken>();
            MaxNumberAccepted = new Semaphore(numConnections, numConnections);
            AsyncSocketUserToken userToken;
            ProtocolMgr oPacketProcess = new ProtocolMgr();
            for (int i = 0; i < NumConnections; i++)
            {
                userToken = new AsyncSocketUserToken(ReceiveBufferSize);
                userToken.AsyncSocket = this;
                userToken.ProtocolMgr = oPacketProcess;
                userToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                AsyncSocketUserTokenPool.Push(userToken);
            }
        }

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            AsyncSocketUserToken userToken = e.UserToken as AsyncSocketUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            try
            {
                lock (userToken)
                {
                    if (e.LastOperation == SocketAsyncOperation.Receive)
                        ProcessReceive(e);
                    else if (e.LastOperation == SocketAsyncOperation.Send)
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
                            CloseSocket(userToken);
                        }
                        else
                        {
                            bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs);
                            if (!willRaiseEvent)
                                ProcessReceive(userToken.ReceiveEventArgs);
                        }
                    }
                    else
                    {
                        bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs);
                        if (!willRaiseEvent)
                            ProcessReceive(userToken.ReceiveEventArgs);
                    }

                }
                else
                {
                    CloseSocket(userToken);
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
                CloseSocket(userToken);
                return false;
            }
        }

        public bool SendAsync(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count)
        {
            if (connectSocket == null) { return false; }
  
            sendEventArgs.SetBuffer(buffer, offset, count);

            bool willRaiseEvent = connectSocket.SendAsync(sendEventArgs);

            if (!willRaiseEvent)
            {
                return ProcessSend(sendEventArgs);
            }
            else
            {
                return true;
            }
        }

        public void CloseSocket(AsyncSocketUserToken userToken)
        {
            if (userToken.ConnectSocket == null)
                return;
            string socketInfo = string.Format("Local Address: {0} Remote Address: {1}", userToken.ConnectSocket.LocalEndPoint,
                userToken.SendEventArgs.RemoteEndPoint);
            Console.WriteLine("Connection disconnected. {0}", socketInfo);
            try
            {
                userToken.ConnectSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception E)
            {
                Console.WriteLine("CloseSocket Disconnect {0} error, message: {1}", socketInfo, E.Message);
            }
            userToken.ConnectSocket.Close();
            userToken.ConnectSocket = null;
            userToken.TokenClose();

            MaxNumberAccepted.Release();
            AsyncSocketUserTokenPool.Push(userToken);
            AsyncSocketUserTokenList.Remove(userToken);
        }

        public void CloseAll()
        {
            AsyncSocketUserToken[] oUserTokens = null;
            AsyncSocketUserTokenList.CopyList(ref oUserTokens);
            for (int i = 0; i < oUserTokens.Length; i++)
            {
                AsyncSocketUserToken oUserToken = oUserTokens[i];
                CloseSocket(oUserToken);
            }
        }
    }
}
