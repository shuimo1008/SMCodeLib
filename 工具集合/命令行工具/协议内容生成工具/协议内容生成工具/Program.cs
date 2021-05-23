using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcDirectory = null;
            string destDirectory = null;
            if (args.Length >= 2)
            {
                srcDirectory = args[0];
                destDirectory = args[1];
            }
            if (string.IsNullOrEmpty(srcDirectory))
            {
                srcDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            if (string.IsNullOrEmpty(destDirectory))
            {
                destDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            Logger.Info("****************************协议处理开始****************************");
            Gen(srcDirectory, destDirectory);
            Logger.Info("****************************协议处理结束****************************");
        }

        public static void Gen(string srcDirectory, string destDirectory)
        {
            try
            {
                string[] files = Directory.GetFiles(srcDirectory, "*.proto");
                if (files.Length == 0)
                {
                    Logger.Error("当前目录下没有找到协议文件: {0}", srcDirectory);
                }
                else
                {
                    IList<StructContainer> structs = StructParser.Process(files);

                    // 生成脚本
                    GenerateCode oGenCode = new GenerateCode();
                    using (FileStream fs = new FileStream(destDirectory + "/Packets.cs", FileMode.Create, FileAccess.Write))
                    {
                        string strScript = oGenCode.Gen(structs);
                        byte[] bytes = Encoding.UTF8.GetBytes(strScript);
                        fs.Write(bytes, 0, bytes.Length);
                    }
                    Logger.Info("代码生成地址:" + destDirectory + "/Packets.cs");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
