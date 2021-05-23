using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Events
{
    public class EventArgs : IEventArgs
    {
        public object Data { get; set; }
    }
}
