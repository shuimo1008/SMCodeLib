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
        /// ʱ���¼
        /// </summary>
        private Time Time { get; set; }
        /// <summary>
        /// ��־���
        /// </summary>
        private Logger Logger { get; set; }
        /// <summary>
        /// ֡ѭ��
        /// </summary>
        private Updater Updater { get; set; }
        /// <summary>
        /// ���߳�
        /// </summary>
        private Mainthread Threader { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        private Bootstrap Bootstraper { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        private Dictionary<Type, object> Instances { get; set; }

        private void Initialize()
        {
            Time = new Time();
            Logger = new Logger();
            Updater = new Updater();
            Threader = new Mainthread();
            Bootstraper = new Bootstrap();
            Instances = new Dictionary<Type, object>();

            ThreadPool.QueueUserWorkItem((_st) =>
            {
                Thread.Sleep(1000);
                if (!Updater.IsUpdate)
                    Error("֡����û�е���,��ÿ֡����App.Update(float deltaTime)����!");
            });
        }

        #region ֡ѭ��
        public static void Update(float deltaTime) => Ins.Updater.Update(deltaTime);
        public static void SubscribeUpdate(Action<float> listener) => Ins.Updater.Add(listener);
        public static void UnsubscribeUpdate(Action<float> listener) => Ins.Updater.Remove(listener);
        #endregion

        #region ��������
        public static Bootstrap Bootstrap(object[] objs)
        {
            if (objs == null) objs = new object[] { };
            for (int i = 0; i < objs.Length; i++)
                if (objs[i] != null) Ins.Bootstraper.Add(objs[i]);
            return Ins.Bootstraper.Startup();
        }

        public static Bootstrap Shutdown()
        {
            return Ins.Bootstraper.Shutdown();
        }
        #endregion

        #region ��־���
        public static void RegistLog(ILogListener listener) => Ins.Logger.Register(listener);
        public static void UnregistLog(ILogListener listener) => Ins.Logger.Unregistter(listener);
        public static void Debug(object msg, params object[] args) => Ins.Logger.Debug(msg, args);
        public static void Warning(object msg, params object[] args) => Ins.Logger.Warning(msg, args);
        public static void Info(object msg, params object[] args) => Ins.Logger.Info(msg, args);
        public static void Error(object msg, params object[] args) => Ins.Logger.Error(msg, args);
        #endregion

        #region �߳�ͬ��
        public static void SendMainthread(SendOrPostCallback callback, object state) => Ins.Threader.Send(callback, state);
        public static void PostMainthread(SendOrPostCallback callback, object state) => Ins.Threader.Post(callback, state);
        #endregion

        #region ����������
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
                    value = ConstructStatic(type, args);
                    Ins.Instances.Add(type, value);
                }
                return value;
            }
        }

        private static object Construct(Type type, params object[] args)
        {
            return ReflUtils.GetConstructor(type, args).Invoke(args);
        }

        private static object ConstructStatic(Type type, params object[] args)
        {
            return Construct(type, args);
        }
        #endregion
    }



    

}