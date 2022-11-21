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
            IoC.Resolve<IDriverSer>()
                .Subscribe(OnUpdate);
        }

        protected abstract void OnUpdate(float deltaTime);

        public virtual void Dispose() 
        {
            IoC.Resolve<IDriverSer>()
                .Unsubscribe(OnUpdate);
            if (View != null) View.Dispose();
        }
    }
}
