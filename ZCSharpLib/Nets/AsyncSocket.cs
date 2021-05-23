using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Nets
{
    public enum Net
    {
        TCP,
        UDP,
    }

    public interface IAsyncScoket
    {
        bool SendAsync(AsyncUserToken userToken);
        void RecvAsync(AsyncUserToken userToken);
        void CloseSocket(AsyncUserToken userToken);
    }
}
