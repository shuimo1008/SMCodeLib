using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string destDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (args.Length >= 1) srcDirectory = srcDirectory + args[0];
            if (args.Length >= 2) destDirectory = destDirectory + args[1];
            try
            {
                Gen(srcDirectory, destDirectory);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        public static void Gen(string srcDirectory, string destDirectory)
        {
            Logger.Explain("配置表示例:", ConsoleColor.Green);
            Logger.Explain("---------------------------", ConsoleColor.Green);
            Logger.Explain("| int | string |  string  |", ConsoleColor.Green);
            Logger.Explain("---------------------------", ConsoleColor.Green);
            Logger.Explain("| TID |  name  | xxxxxxxx |", ConsoleColor.Green);
            Logger.Explain("---------------------------", ConsoleColor.Green);
            Logger.Explain("| TID |  名称  |   xxxx   |", ConsoleColor.Green);
            Logger.Explain("---------------------------", ConsoleColor.Green);
            Logger.Explain("注意:一行一列TID;第一行字段类型;第二行字段名称;第三行字段注释.", ConsoleColor.Green);
            Logger.Explain("配置表描述文件命名:\"_Define_名称.xlsx\"（描述文件不会被处理）.", ConsoleColor.Green);
            Logger.Explain("字段类型支持:int/float/string/int[]/float[]/string[], 数组以\"|\"进行分割.", ConsoleColor.Green);
            Logger.Explain("===================================================================");
            Logger.Explain("配置文件输入目录:"+ srcDirectory);
            Logger.Explain("生成文件输出目录:" + destDirectory);
            Logger.Explain("===================================================================");
            Logger.Info("****************************数据表处理开始****************************");
            try
            {
                List<string> files = new List<string>();
                string[] allFiles = Directory.GetFiles(srcDirectory, "*.xlsx");
                foreach (var file in allFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (!fileName.Contains("_Define_") && !fileName.Contains("~$"))
                        files.Add(file);
                }

                if (files.Count == 0)
                {
                    Logger.Error("当前目录下没有找到配置文件: {0}", srcDirectory);
                }
                else
                {
                    ProcessExcel processExcel = new ProcessExcel();
                    bool isSucess = processExcel.Parse(files.ToArray());
                    isSucess = isSucess & processExcel.Check();
                    if (!isSucess)
                    {
                        Logger.Error("配置表处理出现异常,无法继续生成数据和脚本!");
                    }
                    else
                    {
                        // 生成数据
                        GenerateData oGenData = new GenerateData();
                        using (FileStream fs = new FileStream(destDirectory + "/Template.dat", FileMode.Create, FileAccess.Write))
                        {
                            byte[] bytes = oGenData.Gen(processExcel.FileDatas);
                            fs.Write(bytes, 0, bytes.Length);
                        }

                        // 生成脚本
                        GenerateCode oGenCode = new GenerateCode();
                        using (FileStream fs = new FileStream(destDirectory + "/Template.cs", FileMode.Create, FileAccess.Write))
                        {
                            string strScript = oGenCode.Gen(processExcel.FileDatas);
                            byte[] bytes = Encoding.UTF8.GetBytes(strScript);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                        Logger.Info("**********************************************************************");
                        Logger.Info("代码生成地址:" + destDirectory + "/Template.cs");
                        Logger.Info("数据生成地址:" + destDirectory + "/Template.dat");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message + "\n" + e.StackTrace);
            }

            Logger.Info("****************************数据表处理结束****************************");
        }
    }
}
