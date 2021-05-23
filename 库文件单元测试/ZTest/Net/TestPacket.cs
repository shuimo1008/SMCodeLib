using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZCSharpLib.Net.Interface;
using ZCSharpLib.Net.Packet;

namespace ZTest.Net
{
    public class TestPacket
    {
        public static void Start()
        {
            TestToPacket oPacket = new TestToPacket();
            IPacketCreate oIPacket = oPacket as IPacketCreate;
            IPacket oNewPacket = oIPacket.New() as IPacket;
            Console.WriteLine(oIPacket);
        }
    }

    public class TestToPacket : BasePacket<TestToPacket>
    {

    }
}
