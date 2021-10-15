using System;
using System.Collections.Generic;
using System.Threading;
using ZCSharpLib;
using ZCSharpLib.Nets;
using ZCSharpLib.Nets.TSockets;

namespace ZCSharpLib.Nets.TSockets
{
    public class TSocketServerDaemonThread
    {
        public TSocketServer TCPServer { get; set; }
        public List<AsyncUserToken> UserTokens { get; set; }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(DaemonWork, null);
        }

        public void DaemonWork(object state)
        {
            while (true)
            {
                for (int i = 0; i < UserTokens.Count; i++)
                {
                    try
                    {
                        if ((DateTime.Now - UserTokens[i].ActiveDateTime).Milliseconds > TCPServer.SocketTimeOutMS)
                        {
                            lock (UserTokens[i])
                            {
                                App.Error("连接超时,即将关闭连接TokenID={0}", UserTokens[i].SessionID);
                                TCPServer.CloseSocket(UserTokens[i]);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        App.Error("守护线程检测到Socket通信异常, 异常消息: {0}", e.Message);
                        App.Error(e.StackTrace);
                    }
                }
                Thread.Sleep(10); // 每10毫秒检测一次
            }
        }

        //public void Close()
        //{
        //    Thread.Abort();
        //    Thread.Join();
        //}
    }
}