using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;
using NHibernate.Mapping;

namespace AI.DataExtensions.NHibernate
{
    public interface IObjectConverter
    {
        string ToString(object o);
        object ToObject(Type t, string s);
    }
    public class JsonObjectConverter : IObjectConverter
    {
        public string ToString(object o)
        {
            if (null == o)
                return null;
            return JsonConvert.SerializeObject(o);
           
        }

        public object ToObject(Type t, string s)
        {
            object result = null;
            if (string.IsNullOrEmpty(s))
                return null;
            result = JsonConvert.DeserializeObject(s, t);
            return result;
        }
    }
    public class XmlObjectConverter : IObjectConverter
    {
        public string ToString(object o)
        {
            if (null == o)
                return null;
            string oString = null;
            var serializer = new DataContractJsonSerializer(o.GetType(),KnownTypes(o.GetType()));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, o);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    oString = reader.ReadToEnd();
                }
            }
            return oString;
        }

        private IEnumerable<Type> KnownTypes(Type o)
        {
            var knownTypeAttributes = o.GetCustomAttributes<KnownTypeAttribute>();
            foreach (var attr in knownTypeAttributes)
            {
                yield return attr.Type;
            }
        }
        public T ToObject<T>(string s)
        {
            return (T)ToObject(typeof(T), s);
        }

        public object ToObject(Type t, string s)
        {
            object result = null;
            var serializer = new DataContractJsonSerializer(t, KnownTypes(t));
            var bytes = Encoding.UTF8.GetBytes(s);
            using (var stream = new MemoryStream(bytes))
            {
                result = serializer.ReadObject(stream);
            }
            return result;
        }

    }
}
