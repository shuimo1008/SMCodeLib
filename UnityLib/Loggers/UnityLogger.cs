using SMCore.Logger;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityLib.Loggers
{
    public class UnityLogger : ILoggerListener
    {
        public void Log(LogChannel channel, string msg)
        {
            switch (channel)
            {
                case LogChannel.Debug:
#if WEBGL_DEBUG
                    Debug.Log(msg);
#endif
                    break;
                case LogChannel.Info:
                    Debug.Log(msg);
                    break;
                case LogChannel.Warn:
                    Debug.LogWarning(msg);
                    break;
                case LogChannel.Error:
                    Debug.LogError(msg);
                    break;
            }
        }
    }
}
