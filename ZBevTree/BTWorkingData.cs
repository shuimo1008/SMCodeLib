using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZBevTree
{
    public class BTWorkingData
    {
        public Dictionary<int, BTActionContext> Contexts { get; private set; }

        public BTWorkingData()
        {
            Contexts = new Dictionary<int, BTActionContext>();
        }
    }
}
