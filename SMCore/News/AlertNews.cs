using SMCore.Enums;
using SMCore.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News 
{ 
    public class AlertNews : News, IAlertNews
    {
        public Action OnCancel { get; private set; }
        public Action OnConfirm { get; private set; }

        public IAlertNews SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public IAlertNews OutputNews(string content, bool log = false)
        {
            Content = content;
            if (log) IoC.Resolve<ILoggerSer>().Info(content);
            return this;
        }

        public IAlertNews SetExecute(Func<IAlertNews, ProcessStatus> onProcess)
        {
            return this;
        }

        public IAlertNews SetCancelEvent(Action action)
        {
            OnCancel = action;
            return this;
        }

        public IAlertNews SetConfirmEvent(Action action)
        {
            OnConfirm = action;
            return this;
        }

        public new IAlertNews Publish()
        {
            return IoC.Resolve<INewsSer>().Publish<IAlertNews>(this);
        }

        protected override void OnUpdate(float deltaTime)
        {
        }

        public static AlertNews New() => new AlertNews();
    }
}
