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
using ZCSharpLib.Dialogs;

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
        private Time time;
        public static Time Time => Ins.time;

        /// <summary>
        /// ��־���
        /// </summary>
        private Logger logger;
        public static Logger Logger => Ins.logger;

        private WindowMessage message;
        public static WindowMessage Message => Ins.message;

        /// <summary>
        /// ���߳�
        /// </summary>
        private Mainthread mainthread;
        public static Mainthread Mainthread => Ins.mainthread;


        /// <summary>
        /// ֡ѭ��
        /// </summary>
        private Updater updater;
        /// <summary>
        /// ��������
        /// </summary>
        private Bootstrap bootstrap;
        /// <summary>
        /// ��������
        /// </summary>
        private Dictionary<Type, object> Instances { get; set; }

        private App() { }

        private void Initialize()
        {
            time = new Time();
            logger = new Logger();
            message = new WindowMessage();
            updater = new Updater();
            mainthread = new Mainthread();
            bootstrap = new Bootstrap();
            Instances = new Dictionary<Type, object>();

            ThreadPool.QueueUserWorkItem((_st) =>
            {
                Thread.Sleep(5000);
                if (!updater.IsUpdate)
                    Error("֡����û�е���,��ÿ֡����App.Update(float deltaTime)����!");
            });
        }

        #region ֡ѭ��
        public static void Update(float deltaTime) => Ins.updater.Update(deltaTime);
        public static void SubscribeUpdate(Action<float> listener) => Ins.updater.Add(listener);
        public static void UnsubscribeUpdate(Action<float> listener) => Ins.updater.Remove(listener);
        #endregion

        #region ��������
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

        #region ��־���
        public static void RegistLog(ILogListener listener) => Logger.Register(listener);
        public static void UnregistLog(ILogListener listener) => Logger.Unregistter(listener);
        public static void Debug(object msg) => Logger.Debug(msg);
        public static void Warning(object msg) => Logger.Warning(msg);
        public static void Info(object msg) => Logger.Info(msg);
        public static void Error(object msg) => Logger.Error(msg);
        public static void SetFilter(string[] filters) => Logger.SetFilter(filters);
        #endregion

        #region ��Ϣ����
        public static void SetupDialog(IMessageBox dialog)
        {
            Message.SetupDialog(dialog);
        }
        public static IDialog Show(string title, string message, Action<DialogResult> onResult)
        {
            try{ return Message.Show(title, message, onResult);}
            catch (IMessageBoxIsNullExecption e){Error($"{e}.ͨ��App.SetupDialog����");}
            return null;
        }
        public static IDialog Foucs(string title, string message, Action<DialogResult> onResult)
        {
            try { return Message.Foucs(title, message, onResult); }
            catch (IMessageBoxIsNullExecption e) { Error($"{e}.ͨ��App.SetupDialog����"); }
            return null;
        }
        public static IDialog Alert(string title, string message, Action<DialogResult> onResult)
        {
            try { return Message.Alert(title, message, onResult); }
            catch (IMessageBoxIsNullExecption e) { Error($"{e}.ͨ��App.SetupDialog����"); }
            return null;
        }
        #endregion

        #region �߳�ͬ��
        public static void SendMainthread(SendOrPostCallback callback, object state) => Mainthread.Send(callback, state);
        public static void PostMainthread(SendOrPostCallback callback, object state) => Mainthread.Post(callback, state);
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
                    value = ReflUtils.Construct(type, args);
                    if (value != null) Ins.Instances.Add(type, value);
                }
                return value;
            }
        }
        #endregion
    }



    

}