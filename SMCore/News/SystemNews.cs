using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public class SystemNews : News, ISystemNews
    {
        public ISystemNews SetTitle(string title)
        {
            return this;
        }

        public ISystemNews OutputNews(string content)
        {
            Content = content;
            return this;
        }

        public ISystemNews Publish()
        {
            return IoC.Resolve<INewsSer>().Publish(this);
        }
    }
}
