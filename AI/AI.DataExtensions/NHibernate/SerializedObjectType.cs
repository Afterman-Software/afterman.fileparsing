using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;
using System;
using System.Data;

namespace AI.DataExtensions.NHibernate
{
    public class SerializedObjectType<T, U> : IUserType
        where U : IObjectConverter, new()
    {
        private readonly IObjectConverter _converter;
        public SerializedObjectType()
        {
            _converter = new U();
        }

        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            return x.GetHashCode();
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            var valueToGet = NHibernateUtil.StringClob.NullSafeGet(rs, names[0]);
            if (valueToGet == null || valueToGet.ToString() == String.Empty)
                return null;
            var parts = valueToGet.ToString().Split(new []{"~~"}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return null;
            var type = Type.GetType(parts[0]);
            var val = parts[1];
            return _converter.ToObject(type, val);
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            string valueToSet = String.Empty;
            if (null != value && value.ToString() != String.Empty)
            {
                valueToSet = _converter.ToString(value);
                var type = value.GetType();
                string versionSafeType = string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);

                valueToSet = versionSafeType + "~~" + valueToSet;
            }
            
            NHibernateUtil.StringClob.NullSafeSet(cmd, valueToSet, index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(T); }
        }

        public SqlType[] SqlTypes
        {
            get { return new[] { new SqlType(DbType.String) }; }
        }
    }
}
