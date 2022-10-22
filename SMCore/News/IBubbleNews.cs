using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public interface IBubbleNews
        : INews<IAlertNews>
    {
    }
}
