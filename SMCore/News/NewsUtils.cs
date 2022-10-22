using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public class NewsUtils
    {
        private static INewsSer Srv
        {
            get
            {
                if (_Srv == null)
                {
                    IoC.Register<INewsSer>(() =>
                    {
                        return new NewsSer();
                    });
                    _Srv = IoC.Resolve<INewsSer>();
                }
                return _Srv;
            }
        }
        private static INewsSer _Srv;

        public static void Subscribe<T>(Action<T> subscriber)
            where T : INews => Srv.Subscribe(subscriber);

        public static void Unsubscribe<T>(Action<T> subscriber)
            where T : INews => Srv.Unsubscribe(subscriber);

        public static IAlertNews AlertNews() => new AlertNews();

        public static ISystemNews SystemNew() => new SystemNews();
    }
}
