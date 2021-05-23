using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZUpgrade
{
    public interface IUpgradeListener
    {
        void CallProgram();
        void CallError();
        void OnMessage(string msg);
    }
}
