using System;
using System.Collections.Generic;
using System.Reflection;
using ZCSharpLib.Utils;

namespace ZCSharpLib.Logs
{
	public interface ILogListener
	{
		void Log(LogChannel channel, string msg);
	};

    public enum LogChannel { Debug, Info, Warn, Error, }

    public class Logger
    {
        public int RegisterCount { get; private set; }
        public Action<string> Output { get; private set; }
        private List<ILogListener> Listeners 
        {
            get
            {
                if (_listeners == null)
                    _listeners = new List<ILogListener>();
                return _listeners;
            }
        }
        private List<ILogListener> _listeners;

        public void Register(ILogListener listener)
        {
            if (listener == null)
                return;
            Listeners.Add(listener);
            RegisterCount++;
        }

        public void Unregistter(ILogListener listener)
        {
            if (listener == null)
                return;
            Listeners.Remove(listener);
            RegisterCount--;
        }

        public void Debug(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            Log(str, LogChannel.Debug, false);
        }

        public void Warning(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            Log(str, LogChannel.Warn, false);
        }

        public void Info(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            Log(str, LogChannel.Info, false);
        }

        public void Error(object msg)
        {
            string str = msg != null ? msg.ToString() : "null";
            Log(str, LogChannel.Error, false);
        }

        private void Log(string msg, LogChannel channel, bool simpleMode)
        {
            if (RegisterCount == 0)
                throw new Exception($"日志输出没有监听者(请先通过方法App.RegistLog注册日志监听)!");

            bool filter = false;
            if (filters != null)
            {
                for (int i = 0; i < filters.Length; i++)
                    filter = msg.IndexOf(filters[i]) != -1;
            }
            if (filter) return; // 字段过滤

            string outputMsg;
            if (simpleMode)
                outputMsg = "[" + channel.ToString() + "]  " + msg;
            else
                outputMsg = "[" + channel.ToString() + "][" + DateTime.Now.ToString("F") + "]  " + msg;

            foreach (ILogListener listener in Listeners)
                listener.Log(channel, outputMsg);

            Output?.Invoke(outputMsg);
        }

        private string[] filters;
        public void SetFilter(string[] filters) => this.filters = filters;
    }
}

