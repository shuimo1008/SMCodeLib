using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetSocket
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ProtocolAttribute : Attribute
    {
        public int ID;

        public string Name;

        public ProcessType Type;
    }
}
