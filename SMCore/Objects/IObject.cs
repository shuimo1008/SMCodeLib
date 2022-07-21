using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Objects
{
    public interface IObject : IDisposable
    {
        bool IsDisposed { get; }
    }
}
