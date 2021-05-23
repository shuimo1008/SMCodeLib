using System.Collections;
using System.Threading;
using System.IO;
using System.Net;
using System;
using ZCSharpLib.Comms;

namespace ZCSharpLib.Webs
{
    /// <summary>
    /// 通过http下载资源
    /// </summary>
    public class HttpLoad : IDisposable
    {
        /// <summary>
        /// 下载进度
        /// </summary>
        public float Progress { get; private set; }
        /// <summary>
        /// 资源地址
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// 下载完成后存放路径
        /// </summary>
        public string SavePath { get; private set; }
        /// <summary>
        /// 是否下载完成
        /// </summary>
        public bool IsDone { get; private set; }
        /// <summary>
        /// 是否下载成功
        /// </summary>
        public bool IsSucess { get; private set; }
        /// <summary>
        /// 异常信息
        /// </summary>
        public string Error { get; private set; }
        /// <summary>
        /// 请求
        /// </summary>
        private HttpWebRequest Request { get; set; }
        /// <summary>
        /// 请求响应
        /// </summary>
        private HttpWebResponse Response { get; set; }
        /// <summary>
        /// 没有下载完成时的保存路径
        /// </summary>
        private string TMPSavePath { get; set; }
        /// <summary>
        /// 下载完成后回掉函数
        /// </summary>
        private Action<HttpLoad> OnLoaded { get; set; }
        /// <summary>
        /// 下载进度回掉
        /// </summary>
        private Action<HttpLoad> OnLoading { get; set; }

        // 多线程操作
        public bool IsStart { get; private set; }       //涉及子线程要注意,Unity关闭的时候子线程不会关闭，所以要有一个标识
        private Thread mThread;     //子线程负责下载，否则会阻塞主线程，Unity界面会卡主
        private bool mThreadIsDone = false;
        private bool mThreadIsSucess = false;
        private float mThreadProgress = 0;
        private string mThreadError = string.Empty;

        // 超时时间,当超出当前设定设计没有接受到数据,则认为超时
        private float mTimeout = 15.0f;
        private float mUseTime = 0.0f;

        public HttpLoad(string url, string savePath)
        {
            Url = url;
            SavePath = savePath;
            TMPSavePath = SavePath + ".rc";
        }

        public void SetEventLoaded(Action<HttpLoad> onLoaded)
        {
            OnLoaded = onLoaded;
        }

        public void SetEventLoading(Action<HttpLoad> onLoading)
        {
            OnLoading = onLoading;
        }

        public void Start()
        {
            IsStart = true;
            mThread = new Thread(ThreadLoad);
            mThread.IsBackground = true;
            mThread.Start();
        }

        private void ThreadLoad()
        {
            //判断保存路径是否存在
            string directory = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(SavePath))
            {
                // 文件已经存在则跳过下载
                mThreadError = string.Empty;
                mThreadIsSucess = true;
                mThreadIsDone = true;
                mThreadProgress = 1;
                App.Debug("文件已经存在!无需再次下载保存:{0}", SavePath);
            }
            else
            {
                try
                {
                    long fileLength = 0, totalLength = 0;
                    //使用流操作文件， 新建立一个临时文件，资源下载完成后在把名字改过来
                    using (FileStream fileStream = new FileStream(TMPSavePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        fileLength = fileStream.Length;     //获取文件现在的长度
                        totalLength = GetLength(Url);       //获取下载文件的总长度
                        if (fileLength < totalLength)       //如果没下载完
                        {
                            fileStream.Seek(fileLength, SeekOrigin.Begin);  //断点续传核心，设置本地文件流的起始位置
                            Request = WebRequest.Create(Url) as HttpWebRequest;
                            Request.Timeout = (int)mTimeout * 1000; // timeout设置的毫秒,所以这里需要x1000
                            Request.KeepAlive = true;
                            Request.Proxy = null;
                            
                            // 因为这里是int 类型,所以最高支持2G的断点续传, 如果文件大于2G, 则重新下载的位置也是从2G处开始下载
                            Request.AddRange((int)fileLength);  //断点续传核心，设置远程访问文件流的起始位置
                            Response = Request.GetResponse() as HttpWebResponse;
                            using (Stream stream = Response.GetResponseStream())
                            {
                                byte[] buffer = new byte[1024 * 5];
                                //使用流读取内容到buffer中
                                //注意方法返回值代表读取的实际长度,并不是buffer有多大，stream就会读进去多少
                                int length = stream.Read(buffer, 0, buffer.Length);
                                while (length > 0)
                                {
                                    if (!IsStart)
                                    {
                                        break;  //如果Unity客户端关闭，停止下载
                                    }
                                    fileStream.Write(buffer, 0, length); //将内容再写入本地文件中
                                    fileLength += length;   //计算进度
                                    mThreadProgress = (float)fileLength / (float)totalLength;
                                    length = stream.Read(buffer, 0, buffer.Length); //类似尾递归
                                    mUseTime = 0;
                                }
                            }
                        }
                    };
                    mThreadProgress = (float)fileLength / (float)totalLength;

                    if (mThreadProgress == 1)
                    {
                        // 下载完成后文件改名
                        if (File.Exists(SavePath)) File.Delete(SavePath);
                        FileInfo oFileInfo = new FileInfo(TMPSavePath);
                        oFileInfo.MoveTo(SavePath);
                        if (File.Exists(TMPSavePath)) File.Delete(TMPSavePath);
                        mThreadError = string.Empty;
                        mThreadIsSucess = true;
                        mThreadIsDone = true;
                    }
                    else
                    {
                        if (IsStart)
                        {
                            mThreadError = string.Format("{0}  下载没有完成：{1}", Error, Url);
                        }
                        else
                        {
                            mThreadError = string.Format("{0}  下载被强制关闭：{1}", Error, Url);
                        }
                        mThreadIsSucess = false;
                        mThreadIsDone = true;
                    }
                }
                catch (Exception e)
                {
                    mThreadError = e.ToString();
                    mThreadIsSucess = false;
                    mThreadIsDone = true;
                }
            }
        }

        /// <summary>
        /// 获取下载文件的大小
        /// </summary>
        /// <returns>The length.</returns>
        /// <param name="url">URL.</param>
        private long GetLength(string url)
        {
            HttpWebRequest requet = WebRequest.Create(url) as HttpWebRequest;
            requet.Method = "HEAD";
            HttpWebResponse response = requet.GetResponse() as HttpWebResponse;
            long contentLenght = response.ContentLength;
            return contentLenght;
        }

        public void Loop(float deltaTime)
        {
            IsSucess = mThreadIsSucess;
            Error = mThreadError;
            Progress = mThreadProgress;
            IsDone = mThreadIsDone;

            if (!IsDone)
            {
                mUseTime = mUseTime + deltaTime;
                if (mUseTime >= mTimeout || !string.IsNullOrEmpty(Error))
                {
                    IsDone = true;
                    IsSucess = false;
                    Error = "下载超时: + \n其他异常：" + Error;
                }
            }
            try
            {
                OnLoading?.Invoke(this);
                if (IsDone)
                {
                    OnLoaded?.Invoke(this);
                }
            }
            catch (Exception e)
            {
                App.Error(e);
            }
            if(IsDone) Close();
        }

        public void Close()
        {
            try
            {
                IsStart = false;
                if (Response != null) Response.Close();
                if (Request != null) Request.Abort();
                if (mThread != null) mThread.Abort();
            }
            catch (Exception e)
            {
                App.Error(e.ToString());
            }
        }

        /// <summary>
        /// 清理正在下载或已经下载的资源
        /// </summary>
        public void Clear()
        {
            if (File.Exists(TMPSavePath)) File.Delete(TMPSavePath);
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }

        public void Dispose()
        {
        }
    }
}