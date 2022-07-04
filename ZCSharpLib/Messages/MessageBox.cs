using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Messages
{
    public interface IMessageBox
    {
        IMessage CreateMessage();
    }

    public enum MessageResult
    {
        OK = 1,
        Cancel = 2,
    }

    public enum MessageType
    { 
        Alert,
        Dialog,
        Message,
    }

    public class IMessageBoxIsNullExecption : Exception
    {
        public override string ToString()
        {
            return "MessageBox没有设置IMessageBox接口的实现!";
        }
    }

    public class WindowMessage
    {
        private IMessageBox messageBox;

        public void SetupMessage(IMessageBox messageBox) => this.messageBox = messageBox;

        public IMessage Message(string title, string message, Action<MessageResult> onResult)
        {
            return SetupMessage(title, message, onResult, MessageType.Message);
        }

        public IMessage Alert(string title, string message, Action<MessageResult> onResult)
        {
            return SetupMessage(title, message, onResult, MessageType.Alert);
        }

        public IMessage Dialog(string title, string message, Action<MessageResult> onResult)
        {
            return SetupMessage(title, message, onResult, MessageType.Dialog);
        }

        public IMessage SetupMessage(string title, string message, Action<MessageResult> onResult, MessageType messageType)
        {
            if (messageBox == null) throw new IMessageBoxIsNullExecption();
            return messageBox.CreateMessage()
                .SetTitle(title)
                .SetMessage(message)
                .SetType(messageType)
                .SetCallback(onResult);
        }
    }
}
