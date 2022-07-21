using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMCore.Events
{
    public delegate void RefAction<T1>(ref T1 arg1);
    public delegate void RefAction<T1, T2>(ref T1 arg1, ref T2 arg2);
    public delegate void RefAction<T1, T2, T3>(ref T1 arg1, ref T2 arg2, ref T3 arg3);
    public delegate void RefAction<T1, T2, T3, T4>(ref T1 arg1, ref T2 arg2, ref T3 arg3, ref T4 args);
}
