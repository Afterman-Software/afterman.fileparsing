using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AI.Common.Dynamics;
using AI.TextFileParsing.Interfaces;
using System;

namespace AI.TextFileParsing
{
	public static class TableToTypeTranslator
	{
		public static List<T> GetTypedList<T>(ITable table) where T : class, new()
		{
			List<PropertyInfo> propertyList = TypedPropertyList.GetPropertyList(typeof(T));

			List<T> result = new List<T>();
			foreach (Row row in table.Rows)
			{
				T current = new T();
				foreach (Column column in row.Columns)
				{
					if (!string.IsNullOrWhiteSpace(((Context)column.Context).TargetFieldName))
					{
						PropertyInfo targetPropery = propertyList.SingleOrDefault(pi => pi.Name == ((Context)column.Context).TargetFieldName);
						if (targetPropery != null && targetPropery.CanWrite)
						{
							targetPropery.SetValue(current, column.ActualValue, null);
						}
					}
				}
				result.Add(current);
			}

			return result;
		}

		public static List<CustomDynamicObject> GetDynamicTypedList(ITable table)
		{
			List<CustomDynamicObject> result = new List<CustomDynamicObject>();
			foreach (Row row in table.Rows)
			{
				CustomDynamicObject current = new CustomDynamicObject();
				foreach (Column column in row.Columns)
				{
					if (!string.IsNullOrWhiteSpace(((Context)column.Context).TargetFieldName))
					{
						current.AddPropertyValue(((Context)column.Context).TargetFieldName, column.ActualValue);
					}
				}
				result.Add(current);
			}

			return result;
		}
	}

	public static class RowToTypeTranslator
	{
        public static T GetType<T>(IRow row) where T : class, new()
        {
            return GetType(typeof(T), row) as T;
        }

		public static object GetType(Type t, IRow row) 
		{
			List<PropertyInfo> propertyList = TypedPropertyList.GetPropertyList(t);

			//T result = new T();
            object result = Activator.CreateInstance(t);
			foreach (Column column in row.Columns)
			{
				if (!string.IsNullOrWhiteSpace(((Context)column.Context).TargetFieldName))
				{
					PropertyInfo targetPropery = propertyList.SingleOrDefault(pi => pi.Name == ((Context)column.Context).TargetFieldName);
					if (targetPropery != null && targetPropery.CanWrite)
					{
						targetPropery.SetValue(result, column.ActualValue, null);
					}
				}
			}

			return result;
		}

		public static CustomDynamicObject GetDynamicType(IRow row)
		{
			CustomDynamicObject result = new CustomDynamicObject();
			foreach (Column column in row.Columns)
			{
				if (!string.IsNullOrWhiteSpace(((Context)column.Context).TargetFieldName))
				{
					result.AddPropertyValue(((Context)column.Context).TargetFieldName, column.ActualValue);
				}
			}

			return result;
		}
	}
}
