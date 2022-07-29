using SMCore.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SMCore.Messenger
{
    public interface IMessengerService : IObject
    {

        void Publish(IMessenger messenger);

        void Subscribe(Action<IMessenger> subscriber);
    }
}
