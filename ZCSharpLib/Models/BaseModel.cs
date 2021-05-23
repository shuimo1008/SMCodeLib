using System;
using System.Collections.Generic;
using ZCSharpLib.Cores;
using ZCSharpLib.Events;
using ZCSharpLib.Objects;

namespace ZCSharpLib.Models
{
    public enum ModelStatus
    {
        Add, Remove, Modify, RemoveAll,
    }

    public class ModelEventArgs<T> : IEventArgs
    {
        public T Data { get; set; }
        public ModelStatus Status { get; set; }
    }

    public abstract class Model : ObjectEvent
    {
        public abstract int DataCount { get; }
        public abstract Type DataType { get; }
    }

    public abstract class BaseModel<T> : Model where T : BaseData
    {
        protected Dictionary<string, T> Datas { get; set; }

        public override int DataCount
        {
            get
            {
                if (Datas != null)
                {
                    return Datas.Count;
                }
                return 0;
            }
        }

        public override Type DataType
        {
            get { return typeof(T); }
        }

        protected BaseModel()
        {
            Datas = new Dictionary<string, T>();
        }

        public virtual bool Add(string guid, T t)
        {
            bool isSucess = false;
            if (!Datas.ContainsKey(guid))
            {
                Datas.Add(guid, t);
                isSucess = true;
                Event.Notify(EventCall, new ModelEventArgs<T>() { Data = t, Status = ModelStatus.Add });
            }
            return isSucess;
        }

        public virtual bool Remove(string guid)
        {
            bool isSucess = false;
            if (Datas.TryGetValue(guid, out var t))
            {
                Datas.Remove(guid);
                isSucess = true;
                Event.Notify(EventCall, new ModelEventArgs<T>() { Data = t, Status = ModelStatus.Remove });
                t.Dispose();
            }
            return isSucess;
        }

        public virtual bool RemoveAll()
        {
            foreach (var item in Datas.Values)
                item.Dispose();
            Datas.Clear();
            Event.Notify(EventCall, new ModelEventArgs<T>() { Status = ModelStatus.RemoveAll });
            return true;
        }

        public virtual bool Modify(string guid, Action<T> setter)
        {
            return Modify(guid, setter, out var t);
        }

        public virtual bool Modify(string guid, Action<T> setter, out T t)
        {
            bool isSucess = true;
            if (!Datas.TryGetValue(guid, out var it))
                isSucess = false;
            setter?.Invoke(it); t = it;
            if (isSucess)
            {
                ModelEventArgs<T> oEventArgs = new ModelEventArgs<T>() { Data = it, Status = ModelStatus.Modify };
                Event.Notify(EventCall, oEventArgs);
            }
            return isSucess;
        }

        public virtual T Find(string guid)
        {
            T t;
            if (!Datas.TryGetValue(guid, out t)) { }
            return t;
        }

        public virtual T Find(Predicate<T> match = null)
        {
            T oData = null;
            foreach (var item in Datas.Values)
            {
                bool isMatch = true;
                if (match != null) isMatch = match(item);
                if (isMatch) { oData = item; break; }
            }
            return oData;
        }

        public virtual IList<T> FindAll(Predicate<T> match = null)
        {
            IList<T> list = new List<T>();
            foreach (var item in Datas.Values)
            {
                bool isMatch = true;
                if (match != null) isMatch = match(item);
                if (isMatch) list.Add(item);
            }
            return list;
        }

        public virtual IList<T> FindAll(int index, int count, Predicate<T> match = null)
        {
            IList<T> list = new List<T>();
            int loopIndex = 0;
            foreach (var item in Datas.Values)
            {
                if (loopIndex < index) continue;
                if (loopIndex >= count) break;
                bool isMatch = true;
                if (match != null) isMatch = match(item);
                if (isMatch) list.Add(item);
                loopIndex++;
            }
            return list;
        }
    }
}

