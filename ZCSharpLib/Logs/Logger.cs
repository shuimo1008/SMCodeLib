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

    public enum LogChannel { DEBUG, INFO, WARNING, ERROR, }

    public class Logger
    {
        public int RegisterCount { get; private set; }
        private List<ILogListener> Listeners { get; set; }

        public Logger()
        {
            Listeners = new List<ILogListener>();
            Type[] types = ReflUtils.GetAllTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].GetInterface(typeof(ILogListener).Name)!=null)
                    Register(Activator.CreateInstance(types[i]) as ILogListener);
            }
            if (RegisterCount == 0)
                throw new Exception($"日志输出没有监听者(请先通过方法App.RegistLog注册日志监听)!");
        }

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

        public void Debug(object msg, params object[] args)
        {
            string str = msg != null ? msg.ToString() : "args is null";
            Log(args.Length == 0 ? msg.ToString() : string.Format(str, args), LogChannel.DEBUG, false);
        }

        public void Warning(object msg, params object[] args)
        {
            string str = msg != null ? msg.ToString() : "args is null";
            Log(args.Length == 0 ? msg.ToString() : string.Format(str, args), LogChannel.WARNING, false);
        }

        public void Info(object msg, params object[] args)
        {
            string str = msg != null ? msg.ToString() : "args is null";
            Log(args.Length == 0 ? msg.ToString() : string.Format(str, args), LogChannel.INFO, false);
        }

        public void Error(object msg, params object[] args)
        {
            string str = msg != null ? msg.ToString() : "args is null";
            Log(args.Length == 0 ? msg.ToString() : string.Format(str, args), LogChannel.ERROR, false);
        }

        private void Log(string msg, LogChannel channel, bool simpleMode)
        {
            string outputMsg;
            if (simpleMode)
                outputMsg = "[" + channel.ToString() + "]  " + msg;
            else
                outputMsg = "[" + channel.ToString() + "][" + DateTime.Now.ToString() + "]  " + msg;

            foreach (ILogListener listener in Listeners)
                listener.Log(channel, outputMsg);
            Console.WriteLine(outputMsg);
        }
    };
}

