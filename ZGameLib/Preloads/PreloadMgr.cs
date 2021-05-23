using System;
using System.Collections;
using System.Collections.Generic;
using ZCSharpLib.Utils;
using ZCSharpLib.Events;
using ZCSharpLib;
using ZCSharpLib.Cores;

namespace ZGameLib.Preloads
{
    public interface IPreload
    {
        bool IsSucess { get; set; }
        IEnumerator Preloading();
    }

    public enum PreloadStatus
    {
        Start,
        Loading,
        Finished,
    }

    public enum PreloadError
    {
        None,
        Already,
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PreloadSortAttribute : Attribute
    {
        public int SortID { get; private set; }

        public PreloadSortAttribute(int iSortID)
        {
            SortID = iSortID;
        }
    }

    public class PreloadMgr : ObjectEvent
    {
        class PreloadCtrl
        {
            public int SortID { get; set; }
            public IPreload IPreload { get; set; }
        }

        public class EventArgs : IEventArgs
        {
            public object Data { get; set; }
            /// <summary>
            /// 已经加载个数
            /// </summary>
            public int Alreay { get; set; }
            /// <summary>
            /// 总共加载个数
            /// </summary>
            public int TotalNum { get; set; }
            /// <summary>
            /// 操作状态
            /// </summary>
            public PreloadStatus Status { get; set; }
            /// <summary>
            /// 异常状态
            /// </summary>
            public PreloadError ErrorStatus { get; set; }
        }

        private List<PreloadCtrl> Preloads { get; set; }
        private bool IsDone { get; set; }

        public PreloadMgr()
        {
            Preloads = new List<PreloadCtrl>();

            Type[] oTypes = ReflUtils.GetAllTypes((oType)=>
            {
                bool isSucess = false; 
                if (oType.IsClass && oType.GetInterface(typeof(IPreload).Name) == typeof(IPreload))
                {
                    isSucess = true;
                }
                return isSucess;
            });
            for (int i = 0; i < oTypes.Length; i++)
            {
                Type oType = oTypes[i];
                IPreload oPreload = Activator.CreateInstance(oType) as IPreload;
                object[] oAttrs = oType.GetCustomAttributes(typeof(PreloadSortAttribute), false);
                PreloadCtrl oPreloadCtrl = new PreloadCtrl();
                oPreloadCtrl.SortID = 99;
                oPreloadCtrl.IPreload = oPreload;
                if (oAttrs.Length > 0)
                {
                    oPreloadCtrl.SortID = (oAttrs[0] as PreloadSortAttribute).SortID;
                }
                Preloads.Add(oPreloadCtrl);
            }

            Preloads.Sort((x, y)=> x.SortID.CompareTo(y.SortID));

            IsDone = true;
        }

        [Bootstrap]
        public IEnumerator Init()
        {
            if (!IsDone)
            {
                Notify(new EventArgs() { ErrorStatus = PreloadError.Already });
                yield break;
            }

            IsDone = false;

            // 开始加载
            Notify(new EventArgs()
            {
                Status = PreloadStatus.Start,
                Alreay = 0,
                TotalNum = Preloads.Count
            });

            int count = 0;
            EventArgs oProgEventArgs = new EventArgs();
            // 加载中
            for (int i = 0; i < Preloads.Count; i++)
            {
                oProgEventArgs.Alreay = i;
                oProgEventArgs.Status = PreloadStatus.Loading;
                Notify(oProgEventArgs);

                IPreload oPreload = Preloads[i].IPreload;
                yield return oPreload.Preloading();
                if (oPreload.IsSucess) count++;
            }

            // 加载完成
            Notify(new EventArgs()
            {
                Alreay = count, Status = PreloadStatus.Finished
            });

            IsDone = true;
        }
    }
}
