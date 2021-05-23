using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Events
{
    public interface IEventListener
    {
        void OnNotify(IEventArgs args);
    }
}
