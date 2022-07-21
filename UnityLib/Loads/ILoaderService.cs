using SMCore.Events;
using SMCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityLib.Loads
{
    public interface ILoaderService
    {
        void Load(string uri, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General);
        void LoadImage(string uri, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General);
        void LoadBundle(string uri, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General);
        void LoadAudio(string uri, Action<IEventArgs> onDone, LoadPriority priority = LoadPriority.General, AudioType audioType = AudioType.MPEG);
        void Unload(string uri);
        void UnloadAll();
    }
}
