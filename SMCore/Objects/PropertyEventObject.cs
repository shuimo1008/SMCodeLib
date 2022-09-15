using SMCore.SUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Objects
{
    public abstract class PropertyEventObject
    {
        protected Dictionary<string, Action> PropertyEvents
        {
            get
            {
                if (_PropertyEvents == null)
                    _PropertyEvents = new Dictionary<string, Action>();
                return _PropertyEvents;
            }
        }
        protected Dictionary<string, Action> _PropertyEvents;

        protected void SetClass<T>(string name, ref T currentValue, T newValue) where T : class
        {
            if (Utils.SetClass(ref currentValue, newValue)) PropertyNotify(name);
        }

        private void PropertyNotify(string name)
        {
            
        }
    }
}
