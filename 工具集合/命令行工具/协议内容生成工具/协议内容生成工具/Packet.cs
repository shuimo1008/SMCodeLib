using System;
using System.Collections.Generic;
using System.Text;

namespace Tools
{
    public struct ReqUserPacket
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; }
        public UserBody Body { get; set; }

        public void Seriale()
        {

        }

        public struct UserBody
        {
            public string LeftLeg { get; set; }
            public string RightLeg { get; set; }
        }
    }
}
