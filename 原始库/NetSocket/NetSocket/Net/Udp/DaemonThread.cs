using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetSocket.Udp
{
    class DaemonThread : Object
    {
        private Thread m_thread;

        public DaemonThread()
        {
            m_thread = new Thread(DaemonThreadStart);
            m_thread.Start();
        }

        public void DaemonThreadStart()
        {
            while (m_thread.IsAlive)
            {
                for (int i = 0; i < 60 * 1000 / 10; i++)
                {
                    if (!m_thread.IsAlive)
                        break;
                    Thread.Sleep(10);
                }
            }
        }

        public void Close()
        {
            m_thread.Abort();
            m_thread.Join();
        }
    }
}
