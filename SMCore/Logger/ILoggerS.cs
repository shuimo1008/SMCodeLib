using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Logger
{
    public interface ILoggerS
    {
        void Register(ILoggerListener listener);

        void Unregistter(ILoggerListener listener);

        void Debug(object msg);

        void Warning(object msg);

        void Info(object msg);

        void Error(object msg);
    }

    public interface ILoggerListener
    {
        void Log(LogChannel channel, string msg);
    };

    public enum LogChannel { Debug, Info, Warn, Error, }
}
