using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZCSharpLib;
using ZCSharpLib.Jsons;

namespace ZCSharpLib.Versions.Installs
{
    public class InstallPackage
    {
        public bool IsSucess { get; private set; }
        public PackageInfo Package { get; private set; }
        private string PackagePath { get; set; }
        private SendOrPostCallback OnFinished { get; set; }

        public InstallPackage(string packagePath)
        {
            PackagePath = packagePath;
        }

        public void SetEventOnFinished(SendOrPostCallback callback)
        {
            OnFinished = callback;
        }

        public void Process()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(OnInstallHandler));
        }

        private void OnInstallHandler(object state)
        {
            bool isSucess = ParsePackageInfo();
            if (isSucess)
            {
                isSucess = isSucess & ExecuteCmd();
            }
            IsSucess = isSucess;
            if (OnFinished != null) App.PostMainthread(OnFinished, this);
        }

        private bool ParsePackageInfo()
        {
            bool isSucess = false;
            string infoPath = PackagePath + "/" + "package.json";
            if (File.Exists(infoPath))
            {
                using (FileStream fs = new FileStream(infoPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fs.Length];
                    string json = Encoding.UTF8.GetString(bytes);
                    Package = JsonUtility.Decode<PackageInfo>(json);
                }
            }
            else
            {
                isSucess = false;
            }
            if (Package == null) isSucess = false;
            return isSucess;
        }

        private bool ExecuteCmd()
        {
            bool isSucess = false;
            string[] cmdStrs = Package.CmdStrs;
            for (int i = 0; i < cmdStrs.Length; i++)
            {
                isSucess = isSucess & ParseCmd(cmdStrs[i]);
            }
            return isSucess;
        }

        private bool ParseCmd(string cmdStr)
        {
            bool isSucess = false;
            try
            {
                string[] strs = cmdStr.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length > 0)
                {
                    string cmd = strs[0];
                    if (cmd.Equals("add"))
                    {
                        if (strs.Length < 3) return isSucess;
                        string src = InstallContext.CurrentDir + "package" + strs[1];
                        if (!IsExists(src)) return isSucess;
                        string dest = InstallContext.CurrentDir + strs[2];
                        string destDir = Path.GetDirectoryName(dest);
                        if (Directory.Exists(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }
                        File.Create(dest);
                        File.Move(src, dest);
                        isSucess = true;
                    }
                    else if (cmd.Equals("delete"))
                    {
                        if (strs.Length < 2) return isSucess;
                        string dest = InstallContext.CurrentDir + strs[1];
                        if (File.Exists(dest)) File.Delete(dest);
                        isSucess = true;
                    }
                    else if (cmd.Equals("modify"))
                    {
                        if (strs.Length < 3) return isSucess;
                        string src = InstallContext.CurrentDir + "package" + strs[1];
                        if (!IsExists(src)) return isSucess;
                        string dest = InstallContext.CurrentDir + strs[2];
                        string destDir = Path.GetDirectoryName(dest);
                        if (Directory.Exists(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }
                        if (File.Exists(dest)) File.Delete(dest);
                        File.Create(dest);
                        File.Move(src, dest);
                        isSucess = true;
                    }
                }
            }
            catch (Exception e)
            {
                App.Error(e);
            }
            return isSucess;
        }

        private bool IsExists(string path)
        {
            return File.Exists(path);
        }
    }

    public class PackageInfo
    {
        public string Version;
        public string Infos;
        public string[] CmdStrs;
    }
}
