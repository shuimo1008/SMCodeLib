using SMCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News 
{ 
    public class AlertNews : News, IAlertNews
    {
        public Action OnCancel { get; private set; }
        public Action OnConfirm { get; private set; }

        private Func<IAlertNews, ProcessStatus> OnExecute { get; set; }

        public IAlertNews SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public IAlertNews OutputNews(string news)
        {
            Content = news;
            return this;
        }

        public IAlertNews SetExecute(Func<IAlertNews, ProcessStatus> onProcess)
        {
            OnExecute = onProcess;
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
            base.Publish();
            return IoC.Resolve<INewsSer>().Publish<IAlertNews>(this);
        }

        protected override void OnUpdate(float deltaTime)
        {
            if(OnExecute?.Invoke(this)== ProcessStatus.Finish)
        }


    }
}
