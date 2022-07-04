using System;

namespace ZCSharpLib.Messages
{
    public interface IMessage
    {
        IMessage SetTitle(string title);
        IMessage SetMessage(string message, bool outlog = false);
        IMessage SetType(MessageType messageType);
        IMessage SetCallback(Action<MessageResult> onResult);
        void Close();
    }


    //public class MessageDialog : IDisposable
    //{
    //    public string Title { get; private set; }
    //    public string Message { get; private set; }
    //    public DialogType DialogType { get; private set; }
    //    public Action<DialogResult> OnResult { get; private set; }
    //    public Action<string> OnUpdate { get; set; }
    //    public Action OnDispose { get; set; }

    //    protected IDialog dialog;

    //    public MessageDialog(string title, string message, Action<DialogResult> onResult, DialogType dialogType)
    //    {
    //        Title = title;
    //        Message = message;
    //        OnResult = onResult;
    //        DialogType = dialogType;
    //    }

    //    public void Setup(IDialog dialog) => this.dialog = dialog;

    //    public void Update(string message)
    //    {
    //        Message = message;
    //        if (dialog == null) 
    //            throw new IMessageBoxIsNullExecption();
    //        dialog.Update(Message);
    //    }

    //    public void Close()
    //    {
    //        if (dialog == null) 
    //            throw new IMessageBoxIsNullExecption();
    //        dialog.Close();
    //        Dispose();
    //    }

    //    public void Dispose(){}
    //}
}
