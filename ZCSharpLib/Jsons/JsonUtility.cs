using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZCSharpLib.Jsons
{
    public class JsonUtility
    {
        private static object _object = new object(); // 线程安全锁

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="json">json数据</param>
        /// <returns>反序列化的类型</returns>
        public static T Decode<T>(string json)
        {
            T t = default(T);

            lock (_object)
            {
                t = SimpleJson.DeserializeObject<T>(json);
            }

            return t;
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item">需要序列化的对象</param>
        /// <returns>json数据</returns>
        public static string Encode(object item)
        {
            string json = string.Empty;

            lock (_object)
            {
                json = SimpleJson.SerializeObject(item);
            }

            return json;
        }
    }
}
