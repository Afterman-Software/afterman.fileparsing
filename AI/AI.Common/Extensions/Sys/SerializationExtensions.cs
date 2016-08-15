using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AI.Common.Extensions.Sys
{
    public static class SerializationExtensions
    {
        public static string SerializeIntoBase64String(this object obj)
        {
            var ret = string.Empty;

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                lock (obj)
                {
                    bf.Serialize(ms, obj);
                }

                ret = Convert.ToBase64String(ms.ToArray());
            }

            return ret;
        }

        public static T DeserializeFromBase64String<T>(string base64Bytes) where T : class
        {
            if (string.IsNullOrEmpty(base64Bytes))
                return null;

            T obj;

            var bf = new BinaryFormatter();
            var bytes = Convert.FromBase64String(base64Bytes);
            using (var ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                obj = bf.Deserialize(ms) as T;
            }

            return obj;
        }
    }
}