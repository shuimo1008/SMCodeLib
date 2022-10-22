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
        {
            Register(new UnityLogger());
        }
    }

    public static class LogS
    { 
        private static ILoggerS LoggerS
        {
            get
            {
                if (_LoggerS == null) { 
                    IoC.Register<ILoggerS>(new UnityLoggerS());
                    _LoggerS = IoC.Resolve<ILoggerS>();
                }
                     
                return _LoggerS;
            }
        }
        private static ILoggerS _LoggerS;

        public static void Debug(object msg) => LoggerS.Debug(msg);

        public static void Warning(object msg) => LoggerS.Warning(msg);

        public static void Info(object msg) => LoggerS.Info(msg);

        public static void Error(object msg) => LoggerS.Error(msg);
    }
}
