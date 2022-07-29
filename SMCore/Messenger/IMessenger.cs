using SMCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Messenger
{
    public enum MessengerStyle
    {
        Toast,      // 消息持续几秒自动消失
        Dialog,     // 消息需要用户选择操作
        System,     // 系统消息用户不能操作
        Actively,   // 需要用户激活的操作(一个按钮)
    }

    public enum MessengerResult { Ok, Cancle, }

    public interface IMessenger : IObject
    {
        string Title { get; set; }
        string Content { get; set; }
        MessengerStyle Style { get; }
        Action<MessengerResult> OnResult { get; set; }
        Action OnDisposed { get; set; }
    }
}
