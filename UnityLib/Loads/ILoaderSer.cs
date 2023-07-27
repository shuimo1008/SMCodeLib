using SMCore.Events;
using SMCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

namespace UnityLib.Loads
{
    public interface ILoaderSer
    {
        IReadOnlyDictionary<string, ILoader> ToBeloads { get; }

        void Load(AssetContext info, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadImage(AssetContext info, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadBundle(AssetContext info, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadAudio(AssetContext info, Action<IEventArgs> onDone, Priority priority = Priority.General, AudioType audioType = AudioType.MPEG);

        void Unload(string url);
        void UnloadAll();

        /// <summary>
        /// 加载队列中的操作事件
        /// </summary>
        void AddBeloadsOpEventListener(Action<Opload> listener);

        void RemoveBeloadsOpEventListener(Action<Opload> listener);
    }
}
