using SMCore.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.News
{
    public abstract class News : INews
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public INewsView View { get; private set; }

        public void SetupView(INewsView view)
            => View = view;

        public void Publish()
        {
            IoC.Resolve<IDriverS>()
                .Subscribe(OnUpdate);
        }

        protected abstract void OnUpdate(float deltaTime);

        public virtual void Dispose() 
        {
            IoC.Resolve<IDriverS>()
                .Unsubscribe(OnUpdate);
            if (View != null) View.Dispose();
        }
    }
}
