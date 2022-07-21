using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Services
{
    public interface IServiceContainer : IServiceResolve, IServiceRegistry
    {
    }
}
