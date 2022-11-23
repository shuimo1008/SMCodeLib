using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class AssetPackageRefrence : IDisposable
    {
        public int RefrenceCounting
        {
            get => _RefrenceCounting;
        }
        private int _RefrenceCounting;

        public AssetContext Context { get; private set; }

        public AssetPackage AssetPackage { get; private set; }

        private Dictionary<int, Action<AssetPackage>> Listeners
        {
            get
            {
                if (_Listeners == null)
                    _Listeners = new Dictionary<int, Action<AssetPackage>>();
                return _Listeners;
            }
        }
        private Dictionary<int, Action<AssetPackage>> _Listeners;

        public AssetPackageRefrence(AssetContext info, bool isMemory)
        {
            Context = info;
            AssetPackage = new AssetPackage(info, isMemory, OnAsyncUpdate);
        }

        public int Increment()
        {
            Interlocked.Increment(ref _RefrenceCounting);
            return _RefrenceCounting;
        }

        public int Decrement()
        {
            Interlocked.Decrement(ref _RefrenceCounting);
            return _RefrenceCounting;
        }

        public void AddListener(Action<AssetPackage> onAsync)
        {
            int hashId = onAsync.GetHashCode();
            if (!Listeners.TryGetValue(hashId, out var v))
                Listeners.Add(hashId, onAsync);
        }

        public void Notify()
        {
            foreach (var listener in Listeners.Values)
                listener?.Invoke(AssetPackage);

            if (AssetPackage.IsDone && Listeners.Count > 0)
                Listeners.Clear();
        }

        private void OnAsyncUpdate(AssetPackage package)
        {
            Notify();
        }

        public void Dispose() => AssetPackage.Dispose();
    }
}
