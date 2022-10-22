using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public interface ISystemNews
        : INews<ISystemNews>
    {
        ISystemNews OutputNews(string content);
    }
}
