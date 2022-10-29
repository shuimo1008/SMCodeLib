using SMCore.Events;
using SMCore.Services;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UnityLib.Loads
{
    public interface ILoaderSer
    {
        void Load(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void Load(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadImage(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadImage(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadBundle(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadBundle(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General);
        void LoadAudio(string uri, Action<IEventArgs> onDone, Priority priority = Priority.General, AudioType audioType = AudioType.MPEG);
        void LoadAudio(string uri, string version, Action<IEventArgs> onDone, Priority priority = Priority.General, AudioType audioType = AudioType.MPEG);

        void Unload(string uri);
        void UnloadAll();
    }
}
