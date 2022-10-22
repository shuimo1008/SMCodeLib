using SMCore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public interface INews : IDisposable
    {
        string Title { get; }
        string Content { get; }
        INewsView View { get; }

        void SetupView(INewsView view);

    }

    public interface INews<T> : INews
    {
        T SetTitle(string title);
        T OutputNews(string content);
        IBox SetPrepare(Action<IBox> action);
        IBox SetStart(Action<IBox> action);
        IBox SetExecute(Func<IBox, bool> func);
        IBox SetFinish(Action<IBox> action);
        /// <summary>
        /// 异步处理
        /// </summary>
        /// <param name="onExecute">如果返回为Finish, 则销毁当前消息</param>
        /// <returns>返回当前消息对象</returns>
        T SetExecute(Func<T, ProcessStatus> onExecute);
        T Publish();
    }


}
