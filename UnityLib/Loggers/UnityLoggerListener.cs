﻿using SMCore.Logger;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityLib.Loggers
{
    public class UnityLoggerListener : ILoggerListener
    {
        public void Log(LogChannel channel, string msg)
        {
            switch (channel)
            {
                case LogChannel.Debug:
                    Debug.Log(msg);
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
