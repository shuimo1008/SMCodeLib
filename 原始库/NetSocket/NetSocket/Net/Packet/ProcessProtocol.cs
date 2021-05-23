using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSocket
{
    public abstract class ProcessProtocol<T> : IProtocolProcess where T : class, IPacket
    {
        public void ToProcess(object obj)
        {
            Process(obj as T);
        }
        public abstract void Process(T packet);
    }
}
