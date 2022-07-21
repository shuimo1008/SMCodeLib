using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SMCore.Logger;

namespace UnityLib.Loggers
{
    public class UnityLoggerService : LoggerService
    {
        public UnityLoggerService()
        {
            Register(new UnityLogger());
        }
    }
}
