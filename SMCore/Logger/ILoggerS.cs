using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Logger
{
    public interface ILoggerSer
    {
        void Register(ILoggerListener listener);

        void Unregistter(ILoggerListener listener);
        /// <summary>
        /// 打印调试
        /// </summary>
        /// <param name="msg">输入内容</param>
        /// <returns>日志打印时间</returns>
        string Debug(object msg);
        /// <summary>
        /// 打印警告
        /// </summary>
        /// <param name="msg">输入内容</param>
        /// <returns>日志打印时间</returns>
        string Warning(object msg);
        /// <summary>
        /// 打印信息
        /// </summary>
        /// <param name="msg">输入内容</param>
        /// <returns>日志打印时间</returns>
        string Info(object msg);
        /// <summary>
        /// 打印异常
        /// </summary>
        /// <param name="msg">输入内容</param>
        /// <returns>日志打印时间</returns>
        string Error(object msg);
    }

    public interface ILoggerListener
    {
        void Log(LogChannel channel, string msg);
    };

    public enum LogChannel { Debug, Info, Warn, Error, }
}
