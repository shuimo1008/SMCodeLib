using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Nets
{
    public enum NetworkType { TCP, UDP, }

    public enum NetworkStatus 
    { 
        Disconnect, Connecting, Connected,
    }

    public interface IAsyncScoket
    {
        bool SendAsync(AsyncUserToken userToken);
        void RecvAsync(AsyncUserToken userToken);
        void CloseSocket(AsyncUserToken userToken);
    }
}
