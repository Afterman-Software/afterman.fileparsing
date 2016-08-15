using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using AI.Common.Dynamics;

namespace AI.Common.Tables
{
	public static class TableAndTypeConverter
	{
		public static List<T> GetTypedListFromTable<T>(Table table) where T : new()
		{
			if (typeof(T) == typeof(CustomDynamicObject) || typeof(T) == typeof(DynamicObject) || typeof(T) == typeof(ExpandoObject))
				throw new InvalidOperationException("Tables based on types CustomDynamicObject, DynamicObject, or ExpandoObject cannot be converted to a typed List.");
			if (table.IsPivoted)
				throw new InvalidOperationException("Pivoted Tables cannot be converted to a typed List.");

			List<PropertyInfo> propertyList = TypedPropertyList.GetPropertyList(typeof(T));

			List<T> resultList = new List<T>();
			foreach (Row currentRow in table.Rows)
			{
				T currentT = new T();
				foreach (Column currentColumn in currentRow.Columns)
				{
					if (!string.IsNullOrWhiteSpace(currentColumn.Context.PropInfo.Name))
					{
						PropertyInfo targetPropery = propertyList.Where(pi => pi.Name == currentColumn.Context.PropInfo.Name).SingleOrDefault();
						if (targetPropery != null && targetPropery.CanWrite)
						{
							targetPropery.SetValue(currentT, currentColumn.ActualValue, null);
						}
					}
				}
				resultList.Add(currentT);
			}

			return resultList;
		}

		public static Table GetTableFromTypedList<T>(List<T> typedList) where T : new()
		{
			List<PropertyInfo> propertyList;
			Table resultTable = new Table();

			if (typeof(T) != typeof(CustomDynamicObject))
			{
				propertyList = TypedPropertyList.GetPropertyList(typeof(T));

				foreach (T currentT in typedList)
				{
					Row currentRow = new Row(resultTable);
					foreach (PropertyInfo propInfo in propertyList)
					{
						Column currentColumn = null;
						if (!string.IsNullOrWhiteSpace(propInfo.Name))
						{
							if (propInfo.CanRead && propInfo.CanWrite)
							{
								Context context = new Context(propInfo);
								IComparable value = (IComparable)propInfo.GetValue(currentT, null);
								currentColumn = new Column(currentRow, value, context);
							}
						}
						if (currentColumn != null)
							currentRow.Columns.Add(currentColumn);
					}
					if (currentRow.Columns.Count > 0)
						resultTable.Rows.Add(currentRow);
				}
			}
			else
			{
				if (typedList.Count > 0)
				{
					Dictionary<string, Type> propertyDict = (typedList[0] as CustomDynamicObject).GetPropertyNamesAndTypes();

					foreach (T currentT in typedList)
					{
						Row currentRow = new Row(resultTable);
						foreach (KeyValuePair<string, Type> propInfo in propertyDict)
						{
							Column currentColumn = null;
							CreateablePropertyInfo cpi = new CreateablePropertyInfo(propInfo.Key, propInfo.Value);
							Context context = new Context(cpi.ToPropertyInfo());
							IComparable value = (IComparable)((currentT as CustomDynamicObject).GetPropertyValue(propInfo.Key));
							currentColumn = new Column(currentRow, value, context);

							if (currentColumn != null)
								currentRow.Columns.Add(currentColumn);
						}
						if (currentRow.Columns.Count > 0)
							resultTable.Rows.Add(currentRow);
					}
				}
			}

			return resultTable;
		}
	}
}