using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Services
{
    public interface IServiceResolve
    {
        object Resolve(Type type);

        T Resolve<T>();

        object Resolve(string name);

        T Resolve<T>(string name);
    }
}
