using System;
using System.Threading;
using ZCSharpLib.Times;

namespace ZCSharpLib.Threads
{
    public class Mainthread : IDisposable
    {
        public int ThreadID { get; private set; }
        private SynchronizeThread SynchronizeThread { get; set; }

        public Mainthread()
        {
            ThreadID = Thread.CurrentThread.ManagedThreadId;
            SynchronizeThread = new SynchronizeThread();
            App.SubscribeUpdate(Update);
        }
        
        void Update(float deltaTime)
        {
            SynchronizeThread.Execute();
        }

        public void Send(SendOrPostCallback callback, object state)
        {
            SynchronizeThread.Send(callback, state);
        }

        public void Post(SendOrPostCallback callback, object state)
        {
            SynchronizeThread.Post(callback, state);
        }

        public void Dispose() { }
    }
}
