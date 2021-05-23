using System;
using System.Threading;
using ZCSharpLib;
using ZCSharpLib.Networks;

internal class DaemonThread
{
    private Thread Thread { get; set; }

    private IAsyncNet AsyncSocketServer { get; set; }

    public DaemonThread(IAsyncNet asyncSocketServer)
    {
        AsyncSocketServer = asyncSocketServer;
        Thread = new Thread(DaemonThreadStart);
        Thread.Start();
    }

    public void DaemonThreadStart()
    {
        //while (Thread.IsAlive)
        //{
        //    AsyncUserToken[] userTokenArray = null;
        //    AsyncSocketServer.AsyncSocketUserTokenList.CopyList(ref userTokenArray);
        //    for (int i = 0; i < userTokenArray.Length; i++)
        //    {
        //        if (!Thread.IsAlive)
        //        {
        //            break;
        //        }
        //        try
        //        {
        //            if ((DateTime.Now - userTokenArray[i].ActiveDateTime).Milliseconds > AsyncSocketServer.SocketTimeOutMS)
        //            {
        //                lock (userTokenArray[i])
        //                {
        //                    AsyncSocketServer.CloseSocket(userTokenArray[i]);
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            App.Logger.Error("Daemon thread check timeout socket error, message: {0}", e.Message);
        //            App.Logger.Error(e.StackTrace);
        //        }
        //    }
        //    for (int j = 0; j < 6000; j++)
        //    {
        //        if (!Thread.IsAlive)
        //        {
        //            break;
        //        }
        //        Thread.Sleep(10);
        //    }
        //}
    }

    public void Close()
    {
        Thread.Abort();
        Thread.Join();
    }
}
