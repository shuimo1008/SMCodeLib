using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Utils
{
    public class FileUtils
    {
        public static byte[] ReadBytes(string path)
        {
            byte[] bytes = null;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
            }
            return bytes;
        }

        public static string ReadUTF8(byte[] buffer)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 3) return Encoding.UTF8.GetString(buffer);
            byte[] array = new byte[] { 239, 187, 191 };
            if (buffer[0] == array[0] && buffer[1] == array[1] && buffer[2] == array[2])
            {
                return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
            }
            return Encoding.UTF8.GetString(buffer);
        }

        public static string ReadUTF8(string path)
        {
            return ReadUTF8(ReadBytes(path));
        }

        public static void WriteBytes(string path, byte[] bytes)
        {
            if (bytes == null) { throw new NullReferenceException(); }
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteUTF8(string path, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            WriteBytes(path, bytes);
        }

        public static bool GetDirectoryChildren(string path, IList<string> list, string extFilter)
        {
            DirectoryInfo oDir = new DirectoryInfo(path);

            if (oDir.Exists) return false;

            string[] filters = null;

            List<FileInfo> oFileList = new List<FileInfo>();

            if (!string.IsNullOrEmpty(extFilter))
            {
                filters = extFilter.Split(new char[] { '|', ';', '；', ';', ',', '，' });
            }

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    FileInfo[] oFileArray = oDir.GetFiles(filter, SearchOption.AllDirectories);
                    oFileList.AddRange(oFileArray);
                }
            }
            else
            {
                FileInfo[] oFileArray = oDir.GetFiles("*", SearchOption.AllDirectories);
                oFileList.AddRange(oFileArray);
            }

            foreach (var item in oFileList)
            {
                list.Add(item.FullName);
            }

            return true;
        }
    }
}