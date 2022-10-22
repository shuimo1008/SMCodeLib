using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Boxs
{
    public class SystemBox : IBox
    {
        public object WatchVar1 { get; set; }
        public object WatchVar2 { get; set; }
        public object WatchVar3 { get; set; }
        public object WatchVar4 { get; set; }
        public object WatchVar5 { get; set; }

        public IBox OutputNews(string content)
        {
            throw new NotImplementedException();
        }

        public IBox SetExecute(Func<IBox, bool> func)
        {
            throw new NotImplementedException();
        }

        public IBox SetFinish(Action<IBox> action)
        {
            throw new NotImplementedException();
        }

        public IBox SetPrepare(Action<IBox> action)
        {
            throw new NotImplementedException();
        }

        public IBox SetStart(Action<IBox> action)
        {
            throw new NotImplementedException();
        }

        public IBox SetTitle(string title)
        {
            throw new NotImplementedException();
        }

        public IBox Watch<T1>(T1 t1) where T1 : class
        {
            throw new NotImplementedException();
        }

        public IBox Watch<T1, T2>(T1 t1, T2 t2)
            where T1 : class
            where T2 : class
        {
            throw new NotImplementedException();
        }

        public IBox Watch<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            throw new NotImplementedException();
        }

        public IBox Watch<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            throw new NotImplementedException();
        }

        public IBox Watch<T1, T2, T3, T4, T5>(ref T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
