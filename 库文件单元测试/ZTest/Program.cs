using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZTest.Net;

namespace ZTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestZBinaryBuffer.Start();
            //TestSocketServer.Start();
            //TestPacket.Start();
            //TestUdpSocket.Start();
            ZExpression.TestExpression oExpression = new ZExpression.TestExpression();
            oExpression.Start();
        }
    }
}
