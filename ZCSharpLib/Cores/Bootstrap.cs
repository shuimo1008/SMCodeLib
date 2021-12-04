using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ZCSharpLib.Coroutines;

namespace ZCSharpLib.Cores
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class BootstrapAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ShutdownAttribute : Attribute { }

    public class StartupCallback
    {
        public Action OnFinished;
    }

    public class ShutdownCallback
    {
        public Action OnFinished;
    }

    public class Bootstrap
    {
        public class BootObject
        {
            public object obj;
            public MethodInfo startupMehtod;
            public MethodInfo shutdownMethod;
        }

        public Action OnStartupFinished { get; set; }
        public Action OnShutdownFinished { get; set; }
        private Coroutiner CoroutineRuntime { get; set; }
        private List<BootObject> BootObjects { get; set; }

        public Bootstrap()
        {
            CoroutineRuntime = new Coroutiner();
            BootObjects = new List<BootObject>();
        }

        public bool Add(object obj)
        {
            bool isSucess = true;
            BootObject bootObject = BootObjects.Find(t => t.obj == obj);
            if (bootObject == null)
            {
                bootObject = new BootObject() { obj = obj };
                MethodInfo[] methodInfos = obj.GetType().GetMethods(BindingFlags.Public
                    | BindingFlags.NonPublic | BindingFlags.Instance);
                for (int i = 0; i < methodInfos.Length; i++)
                {
                    BootstrapAttribute initMethodAttr = methodInfos[i].GetCustomAttribute<BootstrapAttribute>();
                    if (initMethodAttr != null)
                        bootObject.startupMehtod = methodInfos[i];

                    ShutdownAttribute uninitMethodAttr = methodInfos[i].GetCustomAttribute<ShutdownAttribute>();
                    if (uninitMethodAttr != null)
                        bootObject.shutdownMethod = methodInfos[i];
                }
                BootObjects.Add(bootObject);
            }
            else
                isSucess = false;
            if (!isSucess)
                throw new Exception($"已经包含相同的服务对象[{bootObject.obj.GetType().Name}]!");
            return isSucess;
        }

        public Bootstrap Startup()
        {
            CoroutineRuntime.Start(IEStartup());
            return this;
        }

        private IEnumerator IEStartup()
        {
            yield return null;
            foreach (var item in BootObjects)
            {
                App.Info("初始化:" + item.obj.GetType());
                MethodInfo oMethodInfo = item.startupMehtod;
                if (oMethodInfo != null)
                {
                    object obj = oMethodInfo.Invoke(item.obj, null);
                    if (obj is IEnumerator) yield return obj;
                }
            }
            OnStartupFinished?.Invoke();
        }

        public Bootstrap Shutdown()
        {
            CoroutineRuntime.Start(IEShutdown());
            return this;
        }

        private IEnumerator IEShutdown()
        {
            yield return null;
            App.Info("程序卸载开始...");
            foreach (var item in BootObjects)
            {
                MethodInfo oMethodInfo = item.shutdownMethod;
                if (oMethodInfo != null)
                {
                    object obj = oMethodInfo.Invoke(item.obj, null);
                    App.Info("卸载:" + item.obj.GetType());
                    if (obj is IEnumerator) yield return obj;
                }
            }

            foreach (var item in BootObjects)
            {
                MethodInfo method = item.GetType().GetMethod("Dispose", BindingFlags.Public
                    | BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(item.obj, null);
            }

            App.Info("程序卸载完成...");
            OnShutdownFinished?.Invoke();
        }
    }
}
