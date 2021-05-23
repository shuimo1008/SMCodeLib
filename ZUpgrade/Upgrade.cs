using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZCSharpLib.Features.Web;
using ZCSharpLib.Features.Json;
using ZCSharpLib.Common;
using ZCSharpLib.ZTEvent;
using ZUpgrade.Data;
using ZUpgrade.Install;

namespace ZUpgrade
{
    public class Upgrade
    {
        public const string MESSAGE_THISVERSION = "ThisVersion";
        public const string MESSAGE_NEWVERSION = "NewVersion";
        public const string MESSAGE_ASSETPROGRESS = "AssetProgress";
        public const string MESSAGE_NORMALINFO = "NormalInfo";

        private static IUpgradeListener Listener { get; set; }
        /// <summary>
        /// 当前版本
        /// </summary>
        public Version ThisVersion { get; private set; }
        /// <summary>
        /// 新版本
        /// </summary>
        public Version NewVersion { get; private set; }
        /// <summary>
        /// 当前版本与新版本间隔多少个版本
        /// </summary>
        public List<VersionData> InteralVersions { get; set; }

        public Upgrade()
        {
            InteralVersions = new List<VersionData>();
        }

        public void Start()
        {
            bool isLoadScuess = LoadLocalInfo(Settings.LocalUrl);
            if (isLoadScuess)
            {
                LogMessage(MESSAGE_THISVERSION, ThisVersion.ToString());
                LoadRemoteInfo(Settings.RemoteUrl);
            }
        }

        public bool LoadLocalInfo(string localUrl)
        {
            bool isLoadSucess = false;
            if (!File.Exists(localUrl))
            {
                LogMessage(MESSAGE_NORMALINFO, "没有找到本地版本信息,请与管理员联系!");
                CallError();
            }
            else
            {
                VersionData data = null;
                using (FileStream fs = new FileStream(Settings.LocalUrl, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[fs.Length];
                    string json = Encoding.UTF8.GetString(bytes);
                    data = JsonUtility.Decode<VersionData>(json);
                }
                if (data == null)
                {
                    LogMessage(MESSAGE_NORMALINFO, "无法解析本地版本信息,请与管理员联系!");
                    CallError();
                }
                else
                {
                    ThisVersion = new Version(data.Version);
                    LogMessage(MESSAGE_THISVERSION, ThisVersion.ToString());
                    isLoadSucess = true;
                }
            }
            return isLoadSucess;
        }

        public void LoadRemoteInfo(string upgradeUrl)
        {
            WebNomal oWebNormal = new WebNomal();
            try
            {
                string json = oWebNormal.HttpGet(Settings.RemoteUrl, string.Empty);
                VersionDataCtrl list = JsonUtility.Decode<VersionDataCtrl>(json);
                if (list != null && list.VersionDatas != null && list.VersionDatas.Length > 0)
                {
                    VersionData[] oDatas = list.VersionDatas;
                    if (oDatas.Length == 0)
                    {
                        LogMessage(MESSAGE_NORMALINFO, "服务器版本信息为空,请与管理员联系!程序将在稍后启动.");
                        CallProgram();
                    }
                    else
                    {
                        for (int i = oDatas.Length - 1; i >= 0; i--)
                        {
                            VersionData oData = oDatas[i];
                            if (ThisVersion.Compare(oData.Version) == -1)
                            {
                                InteralVersions.Add(oData);
                            }
                            else break;
                        }
                        NewVersion = new Version(oDatas[oDatas.Length - 1].Version);
                        LogMessage(MESSAGE_NEWVERSION, NewVersion.ToString());
                    }
                    if (InteralVersions.Count == 0)
                    {
                        LogMessage(MESSAGE_NORMALINFO, "当前版本已经是最新版本!");
                        CallProgram();
                    }
                    else { LoadRemoteAsset(); }
                }
                else
                {
                    LogMessage(MESSAGE_NORMALINFO, "服务器版本信息无法处理,请与管理员联系!程序将在稍后启动.");
                    CallProgram();
                }
            }
            catch (Exception e)
            {
                ZLogger.Error(e.Message);
                LogMessage(MESSAGE_NORMALINFO, "服务器版本信息无法处理,请检查网络或与管理员联系!程序将在稍后启动.");
                CallProgram();
            }
        }

        public void LoadRemoteAsset()
        {
            WebAllLoader.WebAssetDir[] oDirs = new WebAllLoader.WebAssetDir[InteralVersions.Count];
            for (int i = oDirs.Length - 1; i >= 0 ; i--)
            {
                VersionData oData = InteralVersions[i];
                WebAllLoader.WebAssetDir oDir = new WebAllLoader.WebAssetDir();
                oDir.Url = Settings.ServerPackageUri + "/" + Path.GetFileNameWithoutExtension(oData.Name) + ".zip";
                oDir.SavePath = Settings.LocalPackageUri + "/package/" + Path.GetFileNameWithoutExtension(oData.Name) + ".zip";
                oDirs[i] = oDir;
            }
            WebAllLoader allLoader = new WebAllLoader();
            allLoader.LoadAll(oDirs);
            allLoader.SetEventAllLoading((_allLoader) =>
            {
                LogMessage(MESSAGE_ASSETPROGRESS, ((int)(_allLoader.FinalProgress * 100)).ToString());
            });
            allLoader.SetEventOnError((_allLoader)=>
            {
                _allLoader.Close();
                LogMessage(MESSAGE_NORMALINFO, "资源下载终止, 稍后将会重新尝试!");
                ZEvent oEvent = new ZEvent();
                oEvent.AddListener(oEvent.GetType().Name, new DelayCaller((_caller) =>
                {
                    LoadRemoteAsset();
                }));
                oEvent.DelayNotify(oEvent.GetType().Name, null, 10);
            });
            allLoader.SetEventAllLoaded((_allLoader)=>
            {
                _allLoader.Close();
                LogMessage(MESSAGE_NORMALINFO, "资源下载完成,等待解压");
                Install(_allLoader.SavePaths);
            });
            allLoader.Start();
        }

        private int mInstallCount = 0;
        private void Install(string[] oPackagePathes)
        {
            Installer installer = new Installer(oPackagePathes);
            installer.SetEventInstalled((_installer) =>
            {
                Installer oInstaller = _installer as Installer;
                if (!oInstaller.IsSucess)
                {
                    if (mInstallCount < 2)
                    {
                        // 如果安装失败则重新加载,一共处理2次
                        LogMessage(MESSAGE_NORMALINFO, "资源安装出错,稍后会重新尝试!");
                        ZEvent oEvent = new ZEvent();
                        oEvent.AddListener(oEvent.GetType().Name, new DelayCaller((_delayCaller) => 
                        {
                            mInstallCount++;
                            LoadRemoteAsset();
                        }));
                        oEvent.DelayNotify(oEvent.GetType().Name, null, 10);
                    }
                    else
                    {
                        LogMessage(MESSAGE_NORMALINFO, "更新失败,请与管理员联系!");
                        CallError();
                    }
                }
                else
                {
                    LogMessage(MESSAGE_NORMALINFO, "更新成功,程序将在稍后启动!");
                    CallProgram();
                }
            });
            installer.Install();
        }

        #region 升级事件监听
        private static void LogMessage(string msgType, string msg)
        {
            if (Listener != null)
            {
                Listener.OnMessage(string.Format("{0}:{1}", msgType, msg));
            }
        }
        private static void CallProgram()
        {
            // 呼叫启动程序
            if (Listener != null)
            {
                Listener.CallProgram();
            }
        }
        private static void CallError()
        {
            if (Listener != null)
            {
                Listener.CallError();
            }
        }
        #endregion
    }
}
