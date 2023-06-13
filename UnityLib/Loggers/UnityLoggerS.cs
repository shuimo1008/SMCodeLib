using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SMCore.Logger;
using SMCore;

namespace UnityLib.Loggers
{
    public class UnityLoggerS : LoggerS
    {
        public UnityLoggerS()
            => Register(new UnityLogger());
    }
}
