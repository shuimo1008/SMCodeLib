using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZUpgrade.Data
{
    public class VersionData
    {
        public string Name;
        public string Version;
        public string MD5;
    }

    public class VersionDataCtrl
    {
        public VersionData[] VersionDatas;
    }
}
