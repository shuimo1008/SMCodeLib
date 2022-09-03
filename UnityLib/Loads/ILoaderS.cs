using SMCore.Events;
using SMCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityLib.Loads
{
    public interface ILoaderS
    {
        void Load(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadImage(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadBundle(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadAudio(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General, AudioType audioType = AudioType.MPEG);
        void Unload(string uri);
        void UnloadAll();
    }
}
