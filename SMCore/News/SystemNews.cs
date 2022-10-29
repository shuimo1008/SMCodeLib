using SMCore.Enums;
using SMCore.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public class SystemNews : News, ISystemNews
    {
        private Func<ISystemNews, ProcessStatus> OnExecute { get; set; }

        public ISystemNews SetTitle(string title)
        {
            return this;
        }

        public ISystemNews OutputNews(string content, bool log = false)
        {
            Content = content;
            if (log) IoC.Resolve<ILoggerS>().Info(content);
            return this;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (OnExecute == null) return;

            if (OnExecute(this) == ProcessStatus.Finish) Dispose();
        }

        public ISystemNews SetExecute(Func<ISystemNews, ProcessStatus> onExecute)
        {
            OnExecute = onExecute;
            return this;
        }

        public new ISystemNews Publish()
        {
            base.Publish();
            return IoC.Resolve<INewsSer>().Publish<ISystemNews>(this);
        }
    }
}
