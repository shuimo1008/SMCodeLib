using System;

namespace ZCSharpLib.Dialogs
{
    public class MessageDialog : IDisposable
    {
        public string Title { get; private set; }
        public string Message { get; private set; }
        public DialogType DialogType { get; private set; }
        public Action<string> OnUpdate { get; set; }
        public Action OnDispose { get; set; }
        public Action<DialogResult> OnResult { get; private set; }

        public MessageDialog(string title, string message, Action<DialogResult> onResult, DialogType dialogType)
        {
            Title = title;
            Message = message;
            OnResult = onResult;
            DialogType = dialogType;
        }

        public void UpdateMessage(string message)
        {
            Message = message;
            OnUpdate?.Invoke(Message);
        }

        public void Dispose()
        {
            OnDispose?.Invoke();
        }
    }
}
