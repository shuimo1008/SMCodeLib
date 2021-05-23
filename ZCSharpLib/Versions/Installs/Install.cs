using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZCSharpLib;

namespace ZCSharpLib.Versions.Installs
{
    public class Install
    {
        public bool IsSucess { get; private set; }
        private Queue<InstallPackage> InstallPackages { get; set; }
        private SendOrPostCallback OnFinished { get; set; }

        public Install(string[] packagePathes)
        {
            InstallPackages = new Queue<InstallPackage>();
            for (int i = 0; i < packagePathes.Length; i++)
            {
                string pathes = packagePathes[i];
                InstallPackage installPackage = new InstallPackage(pathes);
                InstallPackages.Enqueue(installPackage);
            }
        }

        public void SetEventInstalled(SendOrPostCallback callback)
        {
            OnFinished = callback;
        }

        public void GoInstall()
        {
            if (InstallPackages.Count > 0)
            {
                InstallPackage installPackage = InstallPackages.Dequeue();
                installPackage.SetEventOnFinished((_installPackage)=>
                {
                    InstallPackage iinstallPackage = _installPackage as InstallPackage;
                    if (iinstallPackage.IsSucess)
                    {
                        if (InstallPackages.Count > 0) GoInstall();
                        else
                        {
                            IsSucess = true;
                            App.PostMainthread(OnFinished, this);
                        }
                    }
                    else
                    {
                        IsSucess = false;
                        InstallPackages.Clear();
                        App.PostMainthread(OnFinished, this);
                    }
                });
            }
        }
    }
}
