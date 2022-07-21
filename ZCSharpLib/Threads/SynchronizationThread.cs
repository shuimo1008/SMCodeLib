using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZCSharpLib.Threads
{
    public class SynchronizeContext
    {
        private int ThreadID { get; set; }
        private Queue<ThreadNotifer> Notifers { get; set; }

        // Post 同步锁
        private object sync = new object();

        public SynchronizeContext()
        {
            Notifers = new Queue<ThreadNotifer>();
            ThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        public void Send(SendOrPostCallback callback, object state)
        {
            if (ThreadID == Thread.CurrentThread.ManagedThreadId)
                callback(state);
            else
            {
                using (ManualResetEvent manualReset = new ManualResetEvent(false))
                {
                    lock (sync)
                    {
                        Notifers.Enqueue(new ThreadNotifer(callback, state, manualReset));
                    }
                    manualReset.WaitOne();
                }
            }
        }

        public void Post(SendOrPostCallback callback, object state)
        {
            lock (sync)
            {
                Notifers.Enqueue(new ThreadNotifer(callback, state, null));
            }
        }

        public void Execute()
        {
            lock (sync)
            {
                var oCount = Notifers.Count;
                for (int i = 0; i < oCount; i++)
                    Notifers.Dequeue().Invoke();
            }
        }

        private struct ThreadNotifer
        {
            private object State { get; set; }
            private SendOrPostCallback Callback { get; set; }
            private ManualResetEvent WaitHandle { get; set; }

            public ThreadNotifer(SendOrPostCallback callback, object state, ManualResetEvent waitHandle = null)
            {
                Callback = callback;
                State = state;
                WaitHandle = waitHandle;
            }

            public void Invoke()
            {
                Callback(State);
                if (WaitHandle != null) WaitHandle.Set();
            }
        }
    }
}
