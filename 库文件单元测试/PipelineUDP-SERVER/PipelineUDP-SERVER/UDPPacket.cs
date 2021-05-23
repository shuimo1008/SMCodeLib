using PipelineUDP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Packet(Token = "Demo")]
public class UDPPacket : Packet
{
    public string Content;
}
