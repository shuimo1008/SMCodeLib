using SMCore.Services;
using System;

namespace SMCore
{
    public static class IoC
    {
        private static IServiceContainer Container
        {
            get
            {
                if (_Container == null)
                    _Container = new ServiceContainer();
                return _Container;
            }
        }
        private static IServiceContainer _Container;

        public static object Resolve(Type type) => Container.Resolve(type);

        public static object Resolve(string name)=> Container.Resolve(name);

        public static T Resolve<T>()=> Container.Resolve<T>();

        public static T Resolve<T>(string name)=> Container.Resolve<T>(name);

        public static void Register<T>(Func<T> factory) => Container.Register<T>(factory);

        public static void Register(Type type, object target) => Container.Register(type, target);

        public static void Register(string name, object target) => Container.Register(name, target);

        public static void Register<T>(T target) => Container.Register(target);

        public static void Register<T>(string name, Func<T> factory) => Container.Register(name, factory);

        public static void Register<T>(string name, T target) => Container.Register(name, target);

        public static void Unregister(Type type) => Container.Unregister(type);

        public static void Unregister<T>() => Container.Unregister<T>();

        public static void Unregister(string name) => Container.Unregister(name);

        public static T MakeService<T>(Func<T> factory)
        {
            Container.Register(factory);
            return Container.Resolve<T>();
        }
    }
}
