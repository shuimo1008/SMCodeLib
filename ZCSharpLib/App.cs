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
        /// <summary>
        /// ʱ���¼
        /// </summary>
        private static Time mTime = new Time();
        /// <summary>
        /// ��־���
        /// </summary>
        private static Logger mLogger = new Logger();
        /// <summary>
        /// ֡ѭ��
        /// </summary>
        private static Updater mUpdater = new Updater();
        /// <summary>
        /// ���߳�
        /// </summary>
        private static Mainthread mThread = new Mainthread();
        /// <summary>
        /// ��������
        /// </summary>
        private static Bootstrap mBootstrap = new Bootstrap();
        /// <summary>
        /// ��������
        /// </summary>
        private static Dictionary<Type, object> Instances = new Dictionary<Type, object>();

        static App()
        {
            // ���֡������û��ִ��
            ThreadPool.QueueUserWorkItem((_st) =>
            {
                Thread.Sleep(1000);
                if (!mUpdater.IsUpdate)
                    Error("֡����û�е���,������������(Sartup)ǰע��App.Update����(ÿ֡����App.Update����)!");
            });
        }


        #region ֡ѭ��
        public static void Update(float deltaTime) => mUpdater.Update(deltaTime);
        public static void SubscribeUpdate(Action<float> listener) => mUpdater.Add(listener);
        public static void UnsubscribeUpdate(Action<float> listener) => mUpdater.Remove(listener);
        #endregion

        #region ��������
        public static void RegistBoot(object obj) => mBootstrap.Add(obj);

        public static StartupCallback Startup()
        {
            if (mLogger.RegisterCount == 0)
                throw new CustomException("���붩����־:App.RegistLog,����һЩ��ʾ�޷���ʾ!");
            StartupCallback callback = new StartupCallback();
            mBootstrap.OnStartupFinished = callback.OnFinished;
            mBootstrap.Startup();
            return callback;
        }

        public static ShutdownCallback Shutdown()
        {
            ShutdownCallback callback = new ShutdownCallback();
            mBootstrap.OnShutdownFinished = callback.OnFinished;
            mBootstrap.Shutdown();
            return callback;
        }
        #endregion

        #region ��־���
        public static void RegistLog(ILogListener listener) => mLogger.Register(listener);
        public static void UnregistLog(ILogListener listener) => mLogger.Unregistter(listener);
        public static void Debug(object msg, params object[] args) => mLogger.Debug(msg, args);
        public static void Warning(object msg, params object[] args) => mLogger.Warning(msg, args);
        public static void Info(object msg, params object[] args) => mLogger.Info(msg, args);
        public static void Error(object msg, params object[] args) => mLogger.Error(msg, args);
        #endregion

        #region �߳�ͬ��
        public static void SendMainthread(SendOrPostCallback callback, object state) => mThread.Send(callback, state);
        public static void PostMainthread(SendOrPostCallback callback, object state) => mThread.Post(callback, state);
        #endregion

        #region ����������
        public static T MakeSingleton<T>(params object[] args) where T : class
        {
            return MakeSingleton(typeof(T), args) as T;
        }

        public static object MakeSingleton(Type type, params object[] args)
        {
            lock (Instances)
            {
                object value = null;
                if (!Instances.TryGetValue(type, out value))
                {
                    value = ConstructStatic(type, args);
                    Instances.Add(type, value);
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