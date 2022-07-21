using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Events
{
    public interface IEvent
    {

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="listener">事件监听者</param>
        void AddListener(string eventName, Action<IEventArgs> listener);

        /// <summary>
        /// 移除指定的事件
        /// </summary>
        /// <param name="eventName">指定的事件名</param>
        /// <param name="listener">指定的事件</param>
        void RemoveListener(string eventName, Action<IEventArgs> listener);

        /// <summary>
        /// 移除列表中所有给定名称的事件
        /// </summary>
        /// <param name="eventName">指定事件的名称</param>
        void RemoveAllListener(string eventName);

        /// <summary>
        /// 移除所有事件
        /// </summary>
        void RemoveAllListener();

        /// <summary>
        /// 事件通知
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="args">参数</param>
        /// <param name="delayTime"></param>
        void Notify(string eventName, IEventArgs args, float delayTime = 0);
    }
}
