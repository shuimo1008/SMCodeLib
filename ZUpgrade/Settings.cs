using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZUpgrade
{
    public class Settings
    {
        public static string LocalUrl = "";     // 本地地址
        public static string RemoteUrl = "";   // 远程地址

        public static string ServerPackageUri { get; internal set; }
        public static string LocalPackageUri { get; internal set; }

        public static string CurrentDir
        {
            get
            {
                return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }
        }
    }
}
