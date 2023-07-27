using System;
using System.Collections.Generic;
using System.Reflection;

namespace SMCore.Logger
{
    public class LoggerSer : ILoggerSer
    {
        public int RegisterCount { get; private set; }
        public Action<string> Output { get; private set; }
        private List<ILoggerListener> Listeners 
        {
            get
            {
                if (_listeners == null)
                    _listeners = new List<ILoggerListener>();
                return _listeners;
            }
        }
        private List<ILoggerListener> _listeners;

        public void Register(ILoggerListener listener)
        {
            if (listener == null)
                return;
            Listeners.Add(listener);
            RegisterCount++;
        }

        public void Unregistter(ILoggerListener listener)
        {
            if (listener == null)
                return;
            Listeners.Remove(listener);
            RegisterCount--;
        }

        public string Debug(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            return Log(str, LogChannel.Debug);
        }

        public string Warning(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            return Log(str, LogChannel.Warn);
        }

        public string Info(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            return Log(str, LogChannel.Info);
        }

        public string Error(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            return Log(str, LogChannel.Error);
        }

        private string Log(string msg, LogChannel channel)
        {
            if (RegisterCount == 0)
                throw new Exception($"日志输出没有监听者(请先通过方法App.RegistLog注册日志监听)!");

            string logTime = DateTime.Now.ToString("F");

            bool filter = false;
            if (filters != null)
            {
                for (int i = 0; i < filters.Length; i++)
                    filter = msg.IndexOf(filters[i]) != -1;
            }
            if (filter) return logTime; // 字段过滤

            string outputMsg = "[" + channel.ToString() + "][" + logTime + "]  " + msg;

            foreach (ILoggerListener listener in Listeners)
                listener.Log(channel, outputMsg);

            Output?.Invoke(outputMsg);

            return logTime;
        }

        private string[] filters;
        public void SetFilter(string[] filters) => this.filters = filters;
    }
}

