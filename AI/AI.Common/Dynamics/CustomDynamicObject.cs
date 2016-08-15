using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace AI.Common.Dynamics
{
    [Serializable]
    public class CustomDynamicObject : DynamicObject, IEquatable<CustomDynamicObject>
    {
        private readonly Dictionary<string, object> _innerObjectDictionary = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _innerObjectDictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                _innerObjectDictionary.Add(binder.Name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void AddPropertyValue(string key, object value)
        {
            _innerObjectDictionary.Add(key, value);
        }

        public void SetPropertyValue(string key, object value)
        {
            if (_innerObjectDictionary.ContainsKey(key))
            {
                _innerObjectDictionary[key] = value;
            }
            else
            {
                AddPropertyValue(key, value);
            }
        }

        public object GetPropertyValue(string key)
        {
            return _innerObjectDictionary.ContainsKey(key) ? _innerObjectDictionary[key] : null;
        }

        public Dictionary<string, Type> GetPropertyNamesAndTypes()
        {
            return _innerObjectDictionary.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value == null ? null : kvp.Value.GetType()));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(CustomDynamicObject))
                return false;

            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            int resultHashCode = 17;
            unchecked
            {
                foreach (KeyValuePair<string, object> kvp in _innerObjectDictionary)
                {
                    resultHashCode = resultHashCode * 29 + kvp.Key.GetHashCode();
                    if (kvp.Value != null)
                    {
                        resultHashCode = resultHashCode * 29 + kvp.Value.GetHashCode();
                    }
                }
            }
            return resultHashCode;
        }

        public static bool operator ==(CustomDynamicObject a, CustomDynamicObject b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(CustomDynamicObject a, CustomDynamicObject b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            return a.GetHashCode() != b.GetHashCode();
        }

        public bool Equals(CustomDynamicObject other)
        {
            if (other == null)
                return false;
            return GetHashCode() == other.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, object> kvp in _innerObjectDictionary)
            {
                sb.Append(kvp.Key + "=" + kvp.Value + ";");
            }
            return sb.ToString();
        }

        public T ToType<T>() where T : class, new()
        {
            return ToType(typeof(T)) as T;
        }

        public object ToType(Type t)
        {
            object obj = Activator.CreateInstance(t);
            var propertyList = TypedPropertyList.GetPropertyList(t);
            foreach (var property in propertyList)
            {
                var val = this.GetPropertyValue(property.Name);
                property.SetValue(obj, val);
            }
            return obj;
        }
    }
}