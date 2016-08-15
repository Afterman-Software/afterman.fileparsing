using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace AI.DataExtensions.NHibernate
{
    public class ListObjectType<T> : IUserType
    {
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
            var valueToGet = NHibernateUtil.String.NullSafeGet(rs, names[0]);

            if (valueToGet == null || valueToGet.ToString() == String.Empty)
                return new List<T>();

            var value = valueToGet.ToString()
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => (T) Convert.ChangeType(x, typeof(T)))
                .ToList();

            return value;
        }

        public void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            string valueToSet = String.Empty;

            if (null != value)
            {
                var list = value as List<T>;
                if (list != null)
                    valueToSet = String.Join(",", list);
            }
            NHibernateUtil.String.NullSafeSet(cmd, valueToSet, index);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return typeof(List<T>); }
        }

        public SqlType[] SqlTypes
        {
            get { return new[] { new SqlType(DbType.String) }; }
        }
    }
}
