using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ZCSharpLib.Logs;

namespace ZGameLib.Loggers
{
    public class Logger : ILogListener
    {
        public void Log(LogChannel channel, string msg)
        {
            switch (channel)
            {
                case LogChannel.DEBUG:
                case LogChannel.INFO:
                    Debug.Log(msg);
                    break;
                case LogChannel.WARNING:
                    Debug.LogWarning(msg);
                    break;
                case LogChannel.ERROR:
                    Debug.LogError(msg);
                    break;
            }
        }
    }
}
