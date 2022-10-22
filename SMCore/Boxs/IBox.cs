using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Boxs
{
    public interface IBox : IDisposable
    {
        object WatchVar1 { get; set; }
        object WatchVar2 { get; set; }
        object WatchVar3 { get; set; }
        object WatchVar4 { get; set; }
        object WatchVar5 { get; set; }

        IBox SetPrepare(Action<IBox> action);
        IBox SetStart(Action<IBox> action);
        IBox SetExecute(Func<IBox, bool> func);
        IBox SetFinish(Action<IBox> action);
        IBox SetTitle(string title);
        IBox OutputNews(string content);

        IBox Watch<T1>(T1 t1) 
            where T1 : class;
        IBox Watch<T1, T2>(T1 t1, T2 t2)
            where T1 : class
            where T2 : class;
        IBox Watch<T1, T2, T3>(T1 t1, T2 t2, T3 t3)
            where T1 : class
            where T2 : class
            where T3 : class;
        IBox Watch<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class;
        IBox Watch<T1, T2, T3, T4, T5>(ref T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class;
        //IBox Watch<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) where T1 : class;
        //IBox Watch<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) where T1 : class;
        //IBox Watch<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) where T1 : class;
        //IBox Watch<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9) where T1 : class;
        //IBox Watch<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10) where T1 : class;
    }
}
