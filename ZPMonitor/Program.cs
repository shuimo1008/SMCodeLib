using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZCSharpLib.Json;

namespace ZPMonitor
{
    class Program
    {
        private static string RootPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase.Replace("\\", "/");
        private static string LocalPath = RootPath.Substring(0, RootPath.Length - 1);
        private static string BatchMonitorPath = LocalPath + "/monitor.bat" ;
        private static string CfgPath = LocalPath + "/monitor.cfg";
        private static MonitorCfg MonitorCfg;

        static void Main(string[] args)
        {
            //WriteCfg();
            ReadCfg();
            Thread.CurrentThread.IsBackground = true; // 设置当前线程在后台运行
            WindowHide(Console.Title);
            Console.WriteLine(string.Format("正在监控程序: {0}, 进程: {1}", MonitorCfg.AppName, MonitorCfg.ProcessName));
            //5秒后开始运行，接着每隔1秒的调用Tick方法
            Timer tmr = new Timer(Tick, "tick...", MonitorCfg.DelayTime, MonitorCfg.Interval);
            Console.ReadLine();
            tmr.Dispose();
        }

        private static void Tick(object state)
        {
            Thread.CurrentThread.IsBackground = true;

            Process[] localByName = Process.GetProcessesByName(MonitorCfg.ProcessName);

            if (localByName.Length <= 0)
            {
                if (File.Exists(BatchMonitorPath) == true)
                {
                   Process.Start(BatchMonitorPath);
                }
            }
        }

        private static void WriteCfg()
        {
            MonitorCfg = new MonitorCfg();
            MonitorCfg.ProcessName = "xuyouji";
            MonitorCfg.AppName = "虚游记";
            MonitorCfg.DelayTime = 10000;
            MonitorCfg.Interval = 1000;
            MonitorCfg.WriteCfg(CfgPath);
        }

        private static void ReadCfg()
        {
            MonitorCfg = new MonitorCfg();
            MonitorCfg.ReadCfg(CfgPath);
        }

        #region 隐藏窗口  
        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        public static void WindowHide(string consoleTitle)
        {
            IntPtr a = FindWindow("ConsoleWindowClass", consoleTitle);
            if (a != IntPtr.Zero)
                ShowWindow(a, 0);//隐藏窗口  
            else
                throw new Exception("can't hide console window");
        }
        #endregion

    }

    public class MonitorCfg
    {
        /// <summary>
        /// 进程名称
        /// </summary>
        public string ProcessName;

        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName;

        /// <summary>
        /// 延迟多少毫秒开始监控
        /// </summary>
        public int DelayTime;

        /// <summary>
        /// 间隔时间(毫秒)
        /// </summary>
        public int Interval;

        public void WriteCfg(string path)
        {
            string json = JsonUtility.Encode(this);
            File.WriteAllText(path, json);
        }

        public void ReadCfg(string path)
        {
            string json = File.ReadAllText(path);
            MonitorCfg cfg = JsonUtility.Decode<MonitorCfg>(json);
            cfg.CopyTo(this);
        }

        public void CopyTo(MonitorCfg cfg)
        {
            cfg.ProcessName = ProcessName;
            cfg.AppName = AppName;
            cfg.DelayTime = DelayTime;
            cfg.Interval = Interval;
        }
    }
}
