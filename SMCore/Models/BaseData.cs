using SMCore.Objects;
using SMCore.Utils;
using System;
using System.Collections.Generic;

namespace SMCore.Models
{
    public abstract class BaseData : ObjectBase
    {
        protected string _guid;

        public string Guid
        {
            get => _guid;
            protected set => _guid = value;
        }

        public BaseData(string guid) => Guid = guid;
    }

    public abstract class BaseEventData : BaseData
    {
        protected Action<string> Event { get; set; }

        protected void SetClass<T>(string name, ref T currentValue, T newValue) where T : class
        {
            if (CommUtils.SetClass(ref currentValue, newValue)) NotifyPropertyChanged(name);
        }

        protected void SetStruct<T>(string name, ref T currentValue, T newValue) where T : struct
        {
            if (CommUtils.SetStruct(ref currentValue, newValue)) NotifyPropertyChanged(name);
        }

        protected BaseEventData(string guid)
            : base(guid) { }

        public void Watch(Action<string> action) => Event = action;

        private void NotifyPropertyChanged(string name) => Event?.Invoke(name);
    }
}


