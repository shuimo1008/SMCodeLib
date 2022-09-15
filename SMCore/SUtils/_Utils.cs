using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SMCore.SUtils
{
    public class Utils
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

        #region 文件读写
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
        #endregion

        #region 数学操作
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
            return value;
        }
        #endregion

        #region 反射相关
        /// <summary>
        /// 获取所有程序集的符合条件的类型
        /// </summary>
        /// <param name="match">查找条件</param>
        /// <returns>找到的类型</returns>
        public static Type[] GetAllTypes(Predicate<Type> match = null)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly oAssembly = assemblies[i];
                Type[] nTypes = GetTypes(oAssembly, match);
                types.AddRange(nTypes);
            }
            return types.ToArray();
        }

        /// <summary>
        /// 获取指定程序集中符合条件的类型
        /// </summary>
        /// <param name="iAssembly">指定程序集</param>
        /// <param name="match">查找条件</param>
        /// <returns>找到的类型</returns>
        public static Type[] GetTypes(Assembly iAssembly, Predicate<Type> match = null)
        {
            List<Type> types = new List<Type>();
            Type[] oTypes = iAssembly.GetTypes();
            foreach (var item in oTypes)
            {
                bool isSucess = true;
                if (match != null)
                    isSucess = match(item);
                if (isSucess) types.Add(item);
            }
            return types.ToArray();
        }

        public static object Construct(Type type, params object[] args)
        {
            return GetConstructor(type, args).Invoke(args);
        }

        /// <summary>
        /// 获取对应参数的构造函数
        /// </summary>
        /// <param name="type">类</param>
        /// <param name="args">构造参数</param>
        /// <returns></returns>
        public static ConstructorInfo GetConstructor(Type type, params object[] args)
        {
            ConstructorInfo oConstructorInfo = null;
            ConstructorInfo[] oConstructors = type.GetConstructors();
            for (int i = 0; i < oConstructors.Length; i++)
            {
                ConstructorInfo nConstructorInfo = oConstructors[i];
                ParameterInfo[] parameters = nConstructorInfo.GetParameters();
                if (parameters.Length == args.Length)
                {
                    bool flag = true;
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        ParameterInfo parameterInfo = parameters[i];
                        flag = parameterInfo.ParameterType == args[i].GetType();
                        if (!flag) break;
                    }
                    if (flag) oConstructorInfo = nConstructorInfo;
                    if (flag) break;
                }
            }
            if (oConstructorInfo == null)
            {
                throw new ArgumentException(string.Format("没有找到类型{0}的对应构造方法!\n{1}", type.Name, new StackTrace()));
            }
            return oConstructorInfo;
        }

        /// <summary>
        /// 获取所有字段名称
        /// </summary>
        public static string[] GetFields(Type type, BindingFlags bindFlags)
        {
            FieldInfo[] oFieldInfos = type.GetFields(bindFlags);
            string[] oFields = new string[oFieldInfos.Length];
            for (int i = 0; i < oFields.Length; i++)
                oFields[i] = oFieldInfos[i].Name;
            return oFields;
        }

        public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (currentValue.Equals(newValue))
                return false;

            currentValue = newValue;
            return true;
        }

        public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }
        #endregion
    }
}
