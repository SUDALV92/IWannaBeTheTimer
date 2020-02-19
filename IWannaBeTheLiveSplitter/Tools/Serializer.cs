using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace IWannaBeTheLiveSplitter.Tools
{
    public static class Serializer
    {
        public static string Serialize<T>(T item)
        {
            var ms = new MemoryStream();
            new DataContractJsonSerializer(typeof(T)).WriteObject(ms, item);
            return Encoding.UTF8.GetString(ms.ToArray(), 0, ms.ToArray().Length);
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default(T);

            var serializer = new DataContractJsonSerializer(typeof(T));
            var resultClass = (T)serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            return resultClass;
        }
    }
}
