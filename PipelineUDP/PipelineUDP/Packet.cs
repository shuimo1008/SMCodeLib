using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineUDP
{
    public class Packet
    {
        /// <summary>
        /// 包令牌
        /// </summary>
        public string token;
        /// <summary>
        /// 序号
        /// </summary>
        public long sn;
        /// <summary>
        /// 确认号
        /// </summary>
        public byte ack;
        /// <summary>
        /// 包效验等待
        /// </summary>
        public float wait;
    }
}
