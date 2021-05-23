using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZCSharpLib.Threads
{
    public class SynchronizeThread
    {
        private int ThreadID { get; set; }
        private Queue<ThreadItem> Items { get; set; }

        // Post 同步锁
        private object sync = new object();

        public SynchronizeThread()
        {
            Items = new Queue<ThreadItem>();
            ThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        public void Send(SendOrPostCallback callback, object state)
        {
            if (ThreadID == Thread.CurrentThread.ManagedThreadId)
                callback(state);
            else
            {
                using (ManualResetEvent waitHandle = new ManualResetEvent(false))
                {
                    lock (sync)
                    {
                        ThreadItem oWorker = new ThreadItem(callback, state, waitHandle);
                        Items.Enqueue(oWorker);
                    }
                    waitHandle.WaitOne();
                }
            }
        }

        public void Post(SendOrPostCallback callback, object state)
        {
            lock (sync)
            {
                ThreadItem item = new ThreadItem(callback, state, null);
                Items.Enqueue(item);
            }
        }

        public void Execute()
        {
            lock (sync)
            {
                var oCount = Items.Count;
                for (int i = 0; i < oCount; i++)
                {
                    ThreadItem item = Items.Dequeue();
                    item.Invoke();
                }
            }
        }

        private struct ThreadItem
        {
            private object State { get; set; }
            private SendOrPostCallback Callback { get; set; }
            private ManualResetEvent WaitHandle { get; set; }

            public ThreadItem(SendOrPostCallback callback, object state, ManualResetEvent waitHandle = null)
            {
                Callback = callback;
                State = state;
                WaitHandle = waitHandle;
            }

            public void Invoke()
            {
                Callback(State);
                if (WaitHandle != null)
                    WaitHandle.Set();
            }
        }
    }
}
