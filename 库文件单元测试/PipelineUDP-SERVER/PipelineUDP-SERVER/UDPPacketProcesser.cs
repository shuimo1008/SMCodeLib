using PipelineUDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Packet(Token = "Demo")]
public class UDPPacketProcesser : PacketProcesser<UDPPacket>
{
    public override void Process(UDPPacket packet)
    {
        Console.WriteLine(packet.Content);
    }
}
