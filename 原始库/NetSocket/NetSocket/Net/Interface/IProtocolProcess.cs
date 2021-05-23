using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSocket
{
    public interface IProtocolProcess
    {
        void ToProcess(object obj);
    }
}
