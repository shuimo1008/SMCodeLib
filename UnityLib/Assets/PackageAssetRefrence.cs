using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityLib.Loads;

namespace UnityLib.Assets
{
    public class PackageAssetRefrence : IDisposable
    {
        public int RefrenceCounting
        {
            get => _RefrenceCounting;
        }
        private int _RefrenceCounting;

        public AssetContext Context { get; private set; }

        public PackageAsset AssetPackage { get; private set; }

        private Dictionary<int, Action<PackageAsset>> Listeners
        {
            get
            {
                if (_Listeners == null)
                    _Listeners = new Dictionary<int, Action<PackageAsset>>();
                return _Listeners;
            }
        }
        private Dictionary<int, Action<PackageAsset>> _Listeners;

        public PackageAssetRefrence(AssetContext info, bool isMemory)
        {
            Context = info;
            AssetPackage = new PackageAsset(info, isMemory, OnAsyncUpdate);
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

        public void AddListener(Action<PackageAsset> onAsync)
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

        private void OnAsyncUpdate(PackageAsset package)
        {
            Notify();
        }

        public void Dispose() => AssetPackage.Dispose();
    }
}
