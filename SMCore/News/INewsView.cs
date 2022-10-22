using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public interface INewsView : IDisposable
    { 
        
    }

    public interface INewsView<T> : INewsView
        where T : INews
    {
        T News { get; }
    }
}
