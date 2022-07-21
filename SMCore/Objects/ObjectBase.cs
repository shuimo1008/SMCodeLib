using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Objects
{
    public abstract class ObjectBase : IObject
    {
        public bool IsDisposed { get; protected set; }

        public virtual void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                // 那么这个方法是被客户直接调用的,那么托管的,和非托管的资源都可以释放   
                if (disposing)
                {
                    // 释放 托管资源   
                    DoManagedObjectDispose();
                }
                //释放非托管资源   
                DoUnManagedObjectDispose();
                if (disposing) GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

        ~ObjectBase()
        {
            Dispose(false);
        }

        protected virtual void DoManagedObjectDispose() { }

        protected virtual void DoUnManagedObjectDispose() { }
    }
}
