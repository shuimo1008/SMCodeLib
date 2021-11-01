using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Dialogs
{
    public interface IMessageBox
    {
        IDialog CreateDialog();
    }

    public enum DialogResult
    {
        OK = 1,
        Cancel = 2,
    }

    public enum DialogType
    { 
        None,
        Alert,
        Foucs,
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

        public void SetupDialog(IMessageBox dialog) => this.messageBox = dialog;

        public IDialog Foucs(string title, string message, Action<DialogResult> onResult)
        {
            return SetupDialog(title, message, onResult, DialogType.Foucs);
        }

        public IDialog Alert(string title, string message, Action<DialogResult> onResult)
        {
            return SetupDialog(title, message, onResult, DialogType.Alert);
        }

        public IDialog Show(string title, string message, Action<DialogResult> onResult)
        {
            return SetupDialog(title, message, onResult, DialogType.None);
        }

        public IDialog SetupDialog(string title, string message, Action<DialogResult> onResult, DialogType dialogType)
        {
            if (messageBox == null) throw new IMessageBoxIsNullExecption();
            return messageBox.CreateDialog().SetTitle(title).SetMessage(message).SetType(dialogType).SetCallback(onResult);
        }
    }
}
