using System;

namespace SMCore.Core
{
    public class Singleton<T> where T : class, new()
    {
        private static object sync = new object();
        public static T Ins
        {
            get
            {
                lock (sync)
                {
                    if (_ins == null)
                        _ins = new T();
                }
                return _ins;
            }
        }
        private static T _ins = null;
    }
}
