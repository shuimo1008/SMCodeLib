using SMCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Services
{
    public class ServiceContainer : ObjectBase, IServiceContainer
    {
        private Dictionary<string, IFactory> Services
        {
            get
            { 
                if(_Services==null)
                    _Services = new Dictionary<string, IFactory>();
                return _Services;
            }
        }
        private Dictionary<string, IFactory> _Services;

        public virtual object Resolve(Type type)
        {
            return Resolve<object>(type.Name);
        }

        public virtual T Resolve<T>()
        {
            return Resolve<T>(typeof(T).Name);
        }

        public virtual object Resolve(string name)
        {
            return Resolve<object>(name);
        }

        public virtual T Resolve<T>(string name)
        {
            IFactory factory;
            if (Services.TryGetValue(name, out factory))
                return (T)factory.Create();
            throw new UnregisterServiceException($"未注册的服务 {name}");
        }

        public virtual void Register<T>(Func<T> factory)
        {
            Register(typeof(T).Name, factory);
        }

        public virtual void Register(Type type, object target)
        {
            Register<object>(type.Name, target);
        }

        public virtual void Register(string name, object target)
        {
            Register<object>(name, target);
        }

        public virtual void Register<T>(T target)
        {
            Register(typeof(T).Name, target);
        }

        public virtual void Register<T>(string name, Func<T> factory)
        {
            if (Services.ContainsKey(name))
                throw new DuplicateRegisterServiceException($"重复注册的服务 {name}");

            Services.Add(name, new GenericFactory<T>(factory));
        }

        public virtual void Register<T>(string name, T target)
        {
            if (Services.ContainsKey(name))
                throw new DuplicateRegisterServiceException($"重复注册的服务 {name}");

            Services.Add(name, new SingleInstanceFactory(target));
        }

        public virtual void Unregister(Type type)
        {
            Unregister(type.Name);
        }

        public virtual void Unregister<T>()
        {
            Unregister(typeof(T).Name);
        }

        public virtual void Unregister(string name)
        {
            IFactory factory;
            if (Services.TryGetValue(name, out factory))
                factory.Dispose();

            Services.Remove(name);
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();

            foreach (var kv in Services)
                kv.Value.Dispose();
            Services.Clear();
        }

        internal interface IFactory : IDisposable
        {
            object Create();
        }

        internal class GenericFactory<T> : ObjectBase, IFactory
        {
            private Func<T> func;

            private object target;

            public GenericFactory(Func<T> func)
            {
                this.func = func;
            }

            public virtual object Create()
            {
                if (target == null)
                {
                    target = func();
                    return target;
                }
                else return target;
            }

            protected override void DoManagedObjectDispose()
            {
                base.DoManagedObjectDispose();

                var disposable = target as IDisposable;
                if (disposable != null) disposable.Dispose();
                target = null;
            }
        }

        internal class SingleInstanceFactory : ObjectBase, IFactory
        {
            private object target;

            public SingleInstanceFactory(object target)
            {
                this.target = target;
            }

            public virtual object Create() => target;

            protected override void DoManagedObjectDispose()
            {
                base.DoManagedObjectDispose();
                
                var disposable = target as IDisposable;
                if (disposable != null) disposable.Dispose();
                target = null;
            }
        }
    }
}
