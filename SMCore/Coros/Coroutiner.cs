using SMCore.Objects;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SMCore.Coros
{
    /// <summary>
    /// 协程之间不能相互嵌套使用,因为会出现内存溢出.
    /// </summary>
    public class Coroutiner : ObjectBase
    {
        protected Dictionary<IEnumerator, List<Coroutine>> Coroutines { get; set; }

        public Coroutiner()
        {
            Coroutines = new Dictionary<IEnumerator, List<Coroutine>>();
        }

        public Coroutine Start(IEnumerator routine)
        {
            if (routine == null)
                throw new ArgumentException("参数不能为NULL");
            List<Coroutine> oCoroutines = null;
            if (!Coroutines.TryGetValue(routine, out oCoroutines))
            {
                oCoroutines = new List<Coroutine>();
                Coroutines.Add(routine, oCoroutines);
            }
            Coroutine oCoroutine = new Coroutine(routine);
            oCoroutines.Add(oCoroutine);
            oCoroutine.Setup(this);
            oCoroutine.Start();
            return oCoroutine;
        }

        public void Stop(IEnumerator routine)
        {
            List<Coroutine> oCoroutines = null;
            if (Coroutines.TryGetValue(routine, out oCoroutines))
            {
                for (int i = 0; i < oCoroutines.Count; i++)
                {
                    oCoroutines[i].Stop();
                }
                Coroutines.Remove(routine);
            }
        }

        public void Stop(Coroutine routine)
        {
            if (routine == null)
                throw new ArgumentException("参数不能为NULL");
            List<Coroutine> oCoroutines = null;
            if (Coroutines.TryGetValue(routine.Enumerator, out oCoroutines))
            {
                Coroutine oCoroutine = oCoroutines.Find((t) => 
                {
                    return t.Enumerator == routine.Enumerator;
                });
                if (oCoroutine != null) oCoroutine.Stop();
                oCoroutines.Remove(oCoroutine);
                if (oCoroutines.Count == 0)
                {
                    Coroutines.Remove(routine.Enumerator);
                }
            }
        }

        public void StopAll()
        {
            foreach (var item in Coroutines.Values)
            {
                for (int i = 0; i < item.Count; i++)
                    item[i].Stop();
                item.Clear();
            }
            Coroutines.Clear();
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            StopAll();
        }
    }
}
