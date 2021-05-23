using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ZCSharpLib.Utils
{
    public class CommUtils
    {
        /// <summary>
        /// 生成唯一GUID
        /// </summary>
        public static string GenerateGUID()
        {
            // Guid.ToString默认"D"会有连字符,用"N"舍弃连字符"-"
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 生成32位唯一GUID
        /// </summary>
        public static long GenerateGUID32()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// 生成64位唯一GUID
        /// </summary>
        public static long GenerateGUID64()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// 获取时间戳,以毫秒为单位
        /// </summary>
        /// <returns>毫秒</returns>
        public static long GetTimestamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long msdiff = Convert.ToInt64(ts.TotalMilliseconds);
            return msdiff;
        }

        public static string GenerateMD5(string path)
        {
            string MD5 = string.Empty;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
                MD5 = GenerateMD5(bytes);
            }
            return MD5;
        }

        /// <summary>
        /// 生成MD5
        /// </summary>
        /// <returns></returns>
        public static string GenerateMD5(byte[] input)
        {
            string md5Str = string.Empty;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(input);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                md5Str = sb.ToString();
            }
            md5Str = md5Str.Replace("-", "");
            return md5Str;
        }

        public static string Combine(string iPath1, string iPath2)
        {
            return Path.Combine(iPath1, iPath2);
        }
    }
}
