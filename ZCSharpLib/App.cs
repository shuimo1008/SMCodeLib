using System;
using System.Collections.Generic;
using System.Reflection;
using ZCSharpLib.Logs;
using ZCSharpLib.Threads;
using ZCSharpLib.Times;
using ZCSharpLib.Utils;
using System.Collections;
using ZCSharpLib.Coroutines;
using System.Threading;
using ZCSharpLib.Cores;
using ZCSharpLib.Exceptions;

namespace ZCSharpLib
{
    public class App
    {
        private static App Ins
        {
            get
            {
                if (_ins == null) 
                { 
                    _ins = new App();
                    _ins.Initialize();
                }
                return _ins;
            }
        }
        private static App _ins;

        /// <summary>
        /// 时间记录
        /// </summary>
        private Time time;
        public static Time Time => Ins.time;

        /// <summary>
        /// 日志输出
        /// </summary>
        private Logger logger;
        public static Logger Logger => Ins.logger;

        /// <summary>
        /// 主线程
        /// </summary>
        private Mainthread mainthread;
        public static Mainthread Mainthread => Ins.mainthread;


        /// <summary>
        /// 帧循环
        /// </summary>
        private Updater updater;
        /// <summary>
        /// 程序引导
        /// </summary>
        private Bootstrap bootstrap;
        /// <summary>
        /// 单例对象
        /// </summary>
        private Dictionary<Type, object> Instances { get; set; }

        private App() { }

        private void Initialize()
        {
            time = new Time();
            logger = new Logger();
            updater = new Updater();
            mainthread = new Mainthread();
            bootstrap = new Bootstrap();
            Instances = new Dictionary<Type, object>();

            ThreadPool.QueueUserWorkItem((_st) =>
            {
                Thread.Sleep(5000);
                if (!updater.IsUpdate)
                    Error("帧更新没有调用,请每帧调用App.Update(float deltaTime)方法!");
            });
        }

        #region 帧循环
        public static void Update(float deltaTime) => Ins.updater.Update(deltaTime);
        public static void SubscribeUpdate(Action<float> listener) => Ins.updater.Add(listener);
        public static void UnsubscribeUpdate(Action<float> listener) => Ins.updater.Remove(listener);
        #endregion

        #region 启动引导
        public static Bootstrap Bootstrap(object[] objs)
        {
            if (objs == null) objs = new object[] { };
            for (int i = 0; i < objs.Length; i++)
                if (objs[i] != null) Ins.bootstrap.Add(objs[i]);
            return Ins.bootstrap.Startup();
        }

        public static Bootstrap Shutdown()
        {
            return Ins.bootstrap.Shutdown();
        }
        #endregion

        #region 日志输出
        public static void RegistLog(ILogListener listener) => Logger.Register(listener);
        public static void UnregistLog(ILogListener listener) => Logger.Unregistter(listener);
        public static void Debug(object msg, params object[] args) => Logger.Debug(msg, args);
        public static void Warning(object msg, params object[] args) => Logger.Warning(msg, args);
        public static void Info(object msg, params object[] args) => Logger.Info(msg, args);
        public static void Error(object msg, params object[] args) => Logger.Error(msg, args);
        public static void SetFilter(string[] filters) => Logger.SetFilter(filters);
        #endregion

        #region 线程同步
        public static void SendMainthread(SendOrPostCallback callback, object state) => Mainthread.Send(callback, state);
        public static void PostMainthread(SendOrPostCallback callback, object state) => Mainthread.Post(callback, state);
        #endregion

        #region 单例对象构造
        public static T MakeSingleton<T>(params object[] args) where T : class
        {
            return MakeSingleton(typeof(T), args) as T;
        }

        public static object MakeSingleton(Type type, params object[] args)
        {
            lock (Ins.Instances)
            {
                object value = null;
                if (!Ins.Instances.TryGetValue(type, out value))
                {
                    value = ReflectionUtils.Construct(type, args);
                    if (value != null) Ins.Instances.Add(type, value);
                }
                return value;
            }
        }
        #endregion
    }



    

}