using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Versions.Installs
{
    public class InstallData
    {
        public string Name;
        public string Version;
        public string MD5;
    }

    public class VersionDatas
    {
        public InstallData[] Datas;
    }
}
