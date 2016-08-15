using System;
using System.Reflection;

namespace AI.Common.Tables
{
     [Serializable]
	public class Context
	{
		private static Type Nullable_T = typeof(global::System.Nullable<int>).GetGenericTypeDefinition();

		private PropertyInfo _propInfo;

		public Context
			(
				PropertyInfo propInfo
			)
		{
			_propInfo = propInfo;
		}

		public PropertyInfo PropInfo
		{
			get
			{
				return _propInfo;
			}
		}

		public bool IsNullableType()
		{
			return ((_propInfo.PropertyType == typeof(object)) || (_propInfo.PropertyType == typeof(string)) || (_propInfo.PropertyType.IsGenericType && _propInfo.PropertyType.GetGenericTypeDefinition() == Nullable_T));
		}
	}
}