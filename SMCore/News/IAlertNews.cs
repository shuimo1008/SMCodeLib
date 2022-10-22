using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public interface IAlertNews  : INews<IAlertNews>
    {
        Action OnCancel { get; }
        Action OnConfirm { get; }

        IAlertNews SetCancelEvent(Action action);
        IAlertNews SetConfirmEvent(Action action);
    }
}
