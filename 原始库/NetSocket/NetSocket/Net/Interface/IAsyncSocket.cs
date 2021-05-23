using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace NetSocket
{
    public interface IAsyncSocket
    {
        bool SendAsync(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count);
      
    }
}
