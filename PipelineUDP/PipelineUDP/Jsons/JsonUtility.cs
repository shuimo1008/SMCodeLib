using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PipelineUDP.Jsons
{
    public class JsonUtility
    {
        public static T Decode<T>(string json)
        {
            return SimpleJson.DeserializeObject<T>(json);
        }

        public static object Decode(string json, Type type)
        {
            return SimpleJson.DeserializeObject(json, type);
        }

        public static string Encode(object item)
        {
            return SimpleJson.SerializeObject(item);
        }
    }
}
