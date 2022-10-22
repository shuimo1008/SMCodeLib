using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public interface INewsSer
    {
        T Publish<T>(T t) where T : INews;

        void Subscribe<T>(Action<T> subscriber) where T : INews;

        void Unsubscribe<T>(Action<T> subscriber) where T : INews;
    }
}
