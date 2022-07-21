using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMCore.Events
{
    public interface IEventArgs { }

    public interface IEventArgs<T> : IEventArgs
    {
        T Data { get; set; }
    }
}
