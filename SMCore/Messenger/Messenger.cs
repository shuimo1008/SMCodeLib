using SMCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Messenger
{
    public abstract class Messenger : ObjectBase, IMessenger
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public MessengerStyle Style
        {
            get; private set;
        }

        public Action OnDisposed { get; set; }
        public Action<MessengerResult> OnResult { get; set; }

        public Messenger(MessengerStyle style)
        {
            Style = style;
        }

        public IMessenger Publish()
        {
            IoC.Resolve<IMessengerS>().Publish(this);
            return this;
        }

        protected override void DoManagedObjectDispose()
        {
            base.DoManagedObjectDispose();
            OnDisposed?.Invoke();
        }
    }

    public class ToastMessenger : Messenger
    {
        public ToastMessenger()
            : base(MessengerStyle.Toast)
        { }
    }

    public class DailogMessenger : Messenger
    {
        public DailogMessenger()
            : base(MessengerStyle.Dialog)
        { Title = "对话"; }
    }

    public class SystemMessenger : Messenger
    {
        public SystemMessenger()
            : base(MessengerStyle.System)
        { }
    }

    public class ActivelyMessenger : Messenger
    {
        public ActivelyMessenger()
            : base(MessengerStyle.Actively)
        { Title = "提示"; }
    }
}
