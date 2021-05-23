using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZCSharpLib.Utils;
using ZCSharpLib.Zips.Checksum;
using ZCSharpLib.Zips.Zip;

namespace ZCSharpLib.Zips
{
    public abstract class ZipProcess
    {
        public string SrcPath { get; protected set; }
        public string DestPath { get; protected set; }
        public bool IsSucess { get; protected set; }
        public SendOrPostCallback OnFinish { get; set; }
        protected bool IsDeleteDir { get; set; }

        public ZipProcess(string srcPath, string destPath)
        {
            SrcPath = srcPath;
            DestPath = destPath;
        }
        
        public void Start()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Process));
        }

        protected void Process(object state)
        {
            Process();
        }

        protected abstract void Process();

        /// <summary>      
        /// 获取所有文件      
        /// </summary>      
        /// <returns></returns>      
        protected Dictionary<string, DateTime> GetAllFies(string dir)
        {
            Dictionary<string, DateTime> FilesList = new Dictionary<string, DateTime>();
            DirectoryInfo fileDire = new DirectoryInfo(dir);
            if (!fileDire.Exists)
            {
                throw new System.IO.FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
            }
            GetAllDirFiles(fileDire, FilesList);
            GetAllDirsFiles(fileDire.GetDirectories(), FilesList);
            return FilesList;
        }

        /// <summary>      
        /// 获取一个文件夹下的所有文件夹里的文件      
        /// </summary>      
        /// <param name="dirs"></param>      
        /// <param name="filesList"></param>      
        protected void GetAllDirsFiles(DirectoryInfo[] dirs, Dictionary<string, DateTime> filesList)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                foreach (FileInfo file in dir.GetFiles("*.*"))
                {
                    filesList.Add(file.FullName, file.LastWriteTime);
                }
                GetAllDirsFiles(dir.GetDirectories(), filesList);
            }
        }

        /// <summary>      
        /// 获取一个文件夹下的文件      
        /// </summary>      
        /// <param name="dir">目录名称</param>      
        /// <param name="filesList">文件列表HastTable</param>      
        protected void GetAllDirFiles(DirectoryInfo dir, Dictionary<string, DateTime> filesList)
        {
            foreach (FileInfo file in dir.GetFiles("*.*"))
            {
                filesList.Add(file.FullName, file.LastWriteTime);
            }
        }
    }

    public class ZipCompress : ZipProcess
    {
        public int CompressionLevel { get; set; }

        public ZipCompress(string srcPath, string destPath) 
            : base(srcPath, destPath)
        {
            CompressionLevel = 9;
        }

        protected override void Process()
        {
            string targetPath = DestPath + ".tzip.rc";
            try
            {
                using (ZipOutputStream zipoutputstream = new ZipOutputStream(File.Create(DestPath)))
                {
                    zipoutputstream.SetLevel(CompressionLevel);
                    Crc32 crc = new Crc32();
                    Dictionary<string, DateTime> fileList = GetAllFies(SrcPath);
                    foreach (KeyValuePair<string, DateTime> item in fileList)
                    {
                        FileStream fs = File.OpenRead(item.Key.ToString());
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        ZipEntry entry = new ZipEntry(item.Key.Substring(SrcPath.Length));
                        entry.IsUnicodeText = true;
                        entry.DateTime = item.Value;
                        entry.Size = fs.Length;
                        fs.Close();
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        zipoutputstream.PutNextEntry(entry);
                        zipoutputstream.Write(buffer, 0, buffer.Length);
                    }
                }
                IsSucess = true;
            }
            catch (Exception e)
            {
                IsSucess = false;
                App.Error("压缩出错:{0}\n{1}", SrcPath, e.StackTrace);
            }

            if (IsSucess)
            {
                if (IsSucess && IsDeleteDir )
                {
                    if (Directory.Exists(SrcPath)) Directory.Delete(SrcPath, true);
                }
                // 移动文件
                if (!File.Exists(targetPath)) { File.Create(DestPath); }
                File.Move(targetPath, DestPath);
                File.Delete(targetPath);
            }

            if (OnFinish != null)
            {
                App.PostMainthread(OnFinish, this);
            }
        }
    }

    public class ZipDecompress : ZipProcess
    {
        public ZipDecompress(string srcPath, string destPath) 
            : base(srcPath, destPath)
        {
        }
        protected override void Process()
        {
            string directoryName = DestPath + ".tzip.rc";
            try
            {
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);//生成解压目录  
                }
                string CurrentDirectory = directoryName;
                byte[] data = new byte[2048];
                int size = 2048;
                ZipEntry theEntry = null;
                ZipStrings.UseUnicode = true;
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(SrcPath)))
                {
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        theEntry.IsUnicodeText = true;
                        // 该结点是目录  
                        if (theEntry.IsDirectory)
                        {
                            if (!Directory.Exists(CurrentDirectory + "/" + theEntry.Name))
                            {
                                Directory.CreateDirectory(CurrentDirectory + "/" + theEntry.Name);
                            }
                        }
                        else
                        {
                            if (theEntry.Name != String.Empty)
                            {
                                //检查多级目录是否存在    
                                if (theEntry.Name.Contains("/"))
                                {
                                    string parentDirPath = theEntry.Name.Remove(theEntry.Name.LastIndexOf("/") + 1);
                                    if (!Directory.Exists(parentDirPath))
                                    {
                                        Directory.CreateDirectory(CurrentDirectory + "/" + parentDirPath);
                                    }
                                }

                                //解压文件到指定的目录  
                                using (FileStream streamWriter = File.Create(CurrentDirectory + "/" + theEntry.Name))
                                {
                                    while (true)
                                    {
                                        size = s.Read(data, 0, data.Length);
                                        if (size <= 0) break;

                                        streamWriter.Write(data, 0, size);
                                    }
                                    streamWriter.Close();
                                }
                            }
                        }
                    }
                    s.Close();
                }
                IsSucess = true;
            }
            catch (Exception e)
            {
                IsSucess = false;

                App.Error("解压出错:{0}\n{1}", SrcPath, e.ToString());
            }
            if (IsSucess)
            {
                if (IsDeleteDir && Directory.Exists(SrcPath))
                {
                    Directory.Delete(SrcPath, true);
                }
                //if (!Directory.Exists(DestPath)) Directory.CreateDirectory(DestPath);
                Directory.Move(directoryName, DestPath);
                if (Directory.Exists(directoryName))
                {
                    Directory.Delete(directoryName, true);
                }
            }
            if (OnFinish != null)
            {
                App.PostMainthread(OnFinish, this);
            }
        }
    }
}
