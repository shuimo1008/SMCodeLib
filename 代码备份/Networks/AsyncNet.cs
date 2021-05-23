using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Networks
{
    public enum Net
    {
        TCP,
        UDP,
    }

    public interface IAsyncNet
    {
        void Send(Packet iPacket);

        void Recive(Packet iPacket);

        bool SendAsync(AsyncUserToken iSendEventArgs);

        void RecvAsync(AsyncUserToken iRecvEventArgs);

        void CloseSocket(AsyncUserToken iSocketToken);
    }

    public class ClientConnectPacketCreator : IPacketCreator
    {
        public Packet CreatePacket()
        {
            return new ClientConnect();
        }
    }

    [Packet(1)]
    public class ClientConnect : Packet { }

    [Packet(1)]
    public class ClientConnectProtocol : ProtocolProcesser<ClientConnect>
    {
        public override void Process(ClientConnect iPacket)
        {
            if (iPacket.state != null)
            {
                AsyncUserToken oAsyncUserToken = iPacket.state as AsyncUserToken;
                iPacket.OwnerID1 = oAsyncUserToken.OwnerID1;
                oAsyncUserToken.AsyncNet.Send(iPacket);
            }
        }
    }
}
