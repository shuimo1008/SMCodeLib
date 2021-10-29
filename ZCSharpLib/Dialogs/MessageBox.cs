using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZCSharpLib.Dialogs
{
    public interface IDialog
    {
        MessageDialog OnCreate(MessageDialog dialog);
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

    public class NullDialogExecption : Exception
    {
        public override string ToString()
        {
            return "MessageBox没有设置Dialog接口!";
        }
    }

    public class MessageBox
    {
        private IDialog dialog;

        public void SetupDialog(IDialog dialog) => this.dialog = dialog;

        public MessageDialog Foucs(string title, string message, Action<DialogResult> onResult)
        {
            return CreateDialog(title, message, onResult, DialogType.Foucs);
        }

        public MessageDialog Alert(string title, string message, Action<DialogResult> onResult)
        {
            return CreateDialog(title, message, onResult, DialogType.Alert);
        }

        public MessageDialog Show(string title, string message, Action<DialogResult> onResult)
        {
            return CreateDialog(title, message, onResult, DialogType.None);
        }

        public MessageDialog CreateDialog(string title, string message, Action<DialogResult> onResult, DialogType dialogType)
        {
            if (dialog == null) throw new NullDialogExecption();
            return dialog.OnCreate(new MessageDialog(title, message, onResult, dialogType));
        }
    }
}
