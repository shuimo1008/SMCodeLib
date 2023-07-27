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
    public class UnityLoggerSer : LoggerSer
    {
        public static UnityLoggerSer Instance
        {
            get
            { 
                if(instance == null)
                    instance = new UnityLoggerSer();
                return instance;
            }
        }
        public static UnityLoggerSer instance;

        public UnityLoggerSer()
            => Register(new UnityLoggerListener());
    }
}
