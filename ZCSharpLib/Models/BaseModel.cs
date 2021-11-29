using System;
using System.Collections.Generic;
using System.Linq;
using ZCSharpLib.Cores;
using ZCSharpLib.Events;
using ZCSharpLib.Objects;

namespace ZCSharpLib.Models
{
    public enum ModelStatus
    {
        Add, Remove, Modify, RemoveAll,
    }

    public class ModelArgs : IEventArgs
    {
        public object Data { get; set; }
        public ModelStatus Status { get; set; }
    }

    public abstract class Model : ObjectEvent
    {
        public int DataCount => Datas.Count;
        public abstract Type DataType { get; }
        protected Dictionary<string, object> Datas { get; set; }

        public Model()
        {
            Datas = new Dictionary<string, object>();
        }

        public virtual void Add(string guid, object obj)
        {
            if (!Datas.ContainsKey(guid))
            {
                Datas.Add(guid, obj);
                Event.Notify(HashID.ToString(), new ModelArgs() 
                {
                    Data = obj, Status = ModelStatus.Add 
                });
            }
        }

        public virtual void Remove(string guid)
        {
            if (Datas.TryGetValue(guid, out var obj))
            {
                Datas.Remove(guid);
                Event.Notify(HashID.ToString(), new ModelArgs() 
                { 
                    Data = obj, Status = ModelStatus.Remove 
                });
                if (obj is IDisposable v) v.Dispose();
            }
        }

        public virtual void RemoveAll()
        {
            foreach (var obj in Datas.Values)
                if (obj is IDisposable v) v.Dispose();
            Datas.Clear();
            Event.Notify(HashID.ToString(), new ModelArgs() { Status = ModelStatus.RemoveAll });
        }

        public virtual void Modify(string guid, Action<object> modifier)
        {
            if (Datas.TryGetValue(guid, out var obj))
            {
                modifier?.Invoke(obj);
                Event.Notify(HashID.ToString(), new ModelArgs() { Data = obj, Status = ModelStatus.Modify });
            }
        }

        public virtual object Find(Predicate<object> match)
        {
            return Datas.Values.FirstOrDefault(t => match(t));
        }

        public virtual IList<object> FindAll(Predicate<object> match = null)
        {
            return Datas.Values.Where((t) => 
            {
                if (match != null) match(t);
                return true;
            }).ToList();
        }

        public virtual IList<object> FindAll(int index, int count, Predicate<object> match = null)
        {
            return Datas.Values.Skip(index).Where((t)=> 
            {
                if (match != null) match(t);
                return true;
            }).Take(count).ToList();
        }
    }

    public abstract class BaseModel<T> : Model where T : BaseData
    {
        public override Type DataType
        {
            get { return typeof(T); }
        }

        public virtual void Add(string guid, T t)
        {
            base.Add(guid, t);
        }

        public virtual void Modify(string guid, Action<T> modifier)
        {
            base.Modify(guid, (_t)=> { modifier?.Invoke(_t as T); });
        }

        public virtual T Find(string guid)
        {
            return base.Find((_t) => 
            { 
                if (_t is T t) return t.Guid == guid;
                return false;
            }) as T;
        }

        public virtual T Find(Predicate<T> match)
        {
            return base.Find((_t) =>
            {
                if (_t is T t) return match(t);
                return false;
            }) as T;
        }

        public virtual IList<T> FindAll(Predicate<T> match = null)
        {
            return base.FindAll((_t)=>
            {
                if (_t is T t) return match(t);
                return false;
            }).Cast<T>().ToList();
        }

        public virtual IList<T> FindAll(int index, int count, Predicate<T> match = null)
        {
            return base.FindAll(index, count, (_t)=>
            {
                if (_t is T t) return match(t);
                return false;
            }).Cast<T>().ToList();
            //IList<T> list = new List<T>();
            //int loopIndex = 0;
            //foreach (var item in Datas.Values)
            //{
            //    if (loopIndex < index) continue;
            //    if (loopIndex >= count) break;
            //    bool isMatch = true;
            //    if (match != null) isMatch = match(item as T);
            //    if (isMatch) list.Add(item as T);
            //    loopIndex++;
            //}
            //return list;
        }
    }
}

