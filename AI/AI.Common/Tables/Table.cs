using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AI.Common.Dynamics;

namespace AI.Common.Tables
{
     [Serializable]
	public class Table
	{
		private bool _isPivoted = false;

		private Dictionary<string, string> _attributes = new Dictionary<string, string>();

		private List<Row> _rows;

		public Table()
		{
			_rows = new List<Row>();
		}

		public bool IsPivoted
		{
			get
			{
				return _isPivoted;
			}
			internal set
			{
				_isPivoted = value;
			}
		}

		public Dictionary<string, string> Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public List<Row> Rows
		{
			get
			{
				return _rows;
			}
		}


		#region AddColumnToAllRows

		public void AddColumnToAllRows(string name, bool canRead, bool canWrite, Type propertyType, IComparable actualValue = null)
		{
			CreateablePropertyInfo cpi = new CreateablePropertyInfo(name, canRead, canWrite, propertyType);
			Context context = new Context(cpi.ToPropertyInfo());
			foreach (Row row in Rows)
			{
				Column column = new Column(row, actualValue, context);
				row.Columns.Add(column);
			}
		}

		public void AddColumnToAllRows(string name, Type propertyType, IComparable actualValue = null)
		{
			CreateablePropertyInfo cpi = new CreateablePropertyInfo(name, propertyType);
			Context context = new Context(cpi.ToPropertyInfo());
			foreach (Row row in Rows)
			{
				Column column = new Column(row, actualValue, context);
				row.Columns.Add(column);
			}
		}

		public void AddColumnToAllRows(PropertyInfo prototypePropertyInfo, IComparable actualValue = null)
		{
			CreateablePropertyInfo cpi = new CreateablePropertyInfo(prototypePropertyInfo);
			Context context = new Context(cpi.ToPropertyInfo());
			foreach (Row row in Rows)
			{
				Column column = new Column(row, actualValue, context);
				row.Columns.Add(column);
			}
		}

		#endregion

		#region RemoveColumnFromAllRows

		public void RemoveColumnFromAllRows(int columnIndex)
		{
			foreach (Row row in Rows)
			{
				row.Columns.RemoveAt(columnIndex);
			}
		}

		public void RemoveColumnFromAllRows(string columnName)
		{
			int index = GetColumnIndexByName(columnName);
			RemoveColumnFromAllRows(index);
		}

		public void RemoveColumnFromAllRows<T>(Expression<Func<T, object>> columnNameExpression)
		{
			string columnName = Member.Of<T>(columnNameExpression).AsProperty().Name;
			RemoveColumnFromAllRows(columnName);
		}

		#endregion


		public void AddRow(params object[] columnValues)
		{
			int colCount = GetColumnCount();
			Row row = new Row(this);
			Column col;
			for (var i = 0; i < colCount; i++)
			{
				if (columnValues.Length > i)
				{
					col = new Column(row, (IComparable)columnValues[i], Rows[0].Columns[i].Context);
					row.Columns.Add(col);
				}
				else
				{
					Type type = Rows[0].Columns[i].Context.PropInfo.PropertyType;
					object value = columnValues.Last();
					col = new Column(row, (IComparable)value, Rows[0].Columns[i].Context);
					row.Columns.Add(col);
				}
			}
			this.Rows.Add(row);
		}


		public List<Row> GetRowsByIndexes(params int[] indexes)
		{
			List<Row> rowList = new List<Row>();
			if (indexes != null && indexes.Length > 0)
			{
				foreach (int index in indexes)
				{
					Row row = Rows[index];
					rowList.Add(row);
				}
			}
			return rowList;
		}


		#region GetRowsByColumnValue

		public List<Row> GetRowsByColumnValue(int columnIndex, object value)
		{
			List<Row> rowList = new List<Row>();
			foreach (Row row in Rows)
			{
				if (row.Columns[columnIndex].ActualValue.GetHashCode() == value.GetHashCode())
				{
					rowList.Add(row);
				}
			}
			return rowList;
		}

		public List<Row> GetRowsByColumnValue(string columnName, object value)
		{
			int index = GetColumnIndexByName(columnName);
			return GetRowsByColumnValue(index, value);
		}

		public List<Row> GetRowsByColumnValue<T>(Expression<Func<T, object>> columnNameExpression, object value)
		{
			string columnName = Member.Of<T>(columnNameExpression).AsProperty().Name;
			return GetRowsByColumnValue(columnName, value);
		}

		#endregion

		#region GetColumn

		public List<Column> GetColumn(int index)
		{
			List<Column> columnList = new List<Column>();
			if (index > -1)
			{
				foreach (Row row in Rows)
				{
					Column column = row.Columns[index];
					columnList.Add(column);
				}
			}
			return columnList;
		}

		public List<Column> GetColumn(string columnName)
		{
			int index = GetColumnIndexByName(columnName);
			return GetColumn(index);
		}

		public List<Column> GetColumn<T>(Expression<Func<T, object>> columnNameExpression, object value)
		{
			string columnName = Member.Of<T>(columnNameExpression).AsProperty().Name;
			return GetColumn(columnName);
		}

		#endregion


		public int GetColumnCount()
		{
			int result = 0;
			if (Rows.Count > 0)
			{
				result = Rows[0].Columns.Count;
			}
			return result;
		}


		#region GetColumnIndexByName

		public int GetColumnIndexByName(string columnName)
		{
			if (Rows.Count > 0)
			{
				for (int index = 0; index < Rows[0].Columns.Count; index++)
				{
					Column col = Rows[0].Columns[index];
					if (col.Context.PropInfo.Name == columnName)
						return index;
				}
			}
			return -1;
		}

		public int GetColumnIndexByName<T>(Expression<Func<T, object>> columnNameExpression)
		{
			string columnName = Member.Of<T>(columnNameExpression).AsProperty().Name;
			return GetColumnIndexByName(columnName);
		}

		#endregion


		public void MoveColumn(int sourceColumnIndex, int targetColumnIndex)
		{
			if (Rows.Count > 0)
			{
				foreach (Row row in Rows)
				{
					Column colToMove = row.Columns[sourceColumnIndex];
					row.Columns.RemoveAt(sourceColumnIndex);
					row.Columns.Insert(targetColumnIndex, colToMove);
				}
			}
		}


		public void SetAttributeOnRows(string key, string value, params int[] rowIndexes)
		{
			foreach (int rowIndex in rowIndexes)
			{
				Rows[rowIndex].Attributes.Add(key, value);
			}
		}


		public void SetAttributeOnColumns(string key, string value, int[] rowIndexes, params int[] ignoreColumnIndexes)
		{
			foreach (int rowIndex in rowIndexes)
			{
				Row row = Rows[rowIndex];
				int colCount = row.Columns.Count;
				for (int colIndex = 0; colIndex < colCount; colIndex++)
				{
					if (!ignoreColumnIndexes.Contains(colIndex))
					{
						Column col = row.Columns[colIndex];
						col.Attributes.Add(key, value);
					}
				}
			}
		}


		#region SetAttributeOnColumn

		public void SetAttributeOnColumn(int columnIndex, string key, string value, params int[] rowIndexes)
		{
			if (Rows.Count > 0 && columnIndex > -1)
			{
				if (rowIndexes != null && rowIndexes.Length > 0)
				{
					for (int i = 0; i < rowIndexes.Length; i++)
					{
						Row row = Rows[rowIndexes[i]];
						row.Columns[columnIndex].Attributes.Add(key, value);
					}
				}
				else
				{
					foreach (Row row in Rows)
					{
						row.Columns[columnIndex].Attributes.Add(key, value);
					}
				}
			}
		}

		public void SetAttributeOnColumn(string columnName, string key, string value, params int[] rowIndexes)
		{
			int index = GetColumnIndexByName(columnName);
			SetAttributeOnColumn(index, key, value, rowIndexes);
		}

		public void SetAttributeOnColumn<T>(Expression<Func<T, object>> columnNameExpression, string key, string value, params int[] rowIndexes)
		{
			string columnName = Member.Of<T>(columnNameExpression).AsProperty().Name;
			SetAttributeOnColumn(columnName, key, value, rowIndexes);
		}

		#endregion


		public void SetAttributeOnColumn<T2>(int columnIndex, string key, Expression<Func<T2, object>> value, params int[] rowIndexes) where T2 : Row
		{
			Func<T2, object> valueExe = value.Compile();
			if (Rows.Count > 0 && columnIndex > -1)
			{
				if (rowIndexes != null && rowIndexes.Length > 0)
				{
					for (int i = 0; i < rowIndexes.Length; i++)
					{
						Row row = Rows[rowIndexes[i]];
						row.Columns[columnIndex].Attributes.Add(key, valueExe.Invoke((T2)row).ToString());
					}
				}
				else
				{
					foreach (Row row in Rows)
					{
						row.Columns[columnIndex].Attributes.Add(key, valueExe.Invoke((T2)row).ToString());
					}
				}
			}
		}


		public List<string> GetAttributeValuesFromFirstRowColumns(string key)
		{
			List<string> contextValues = new List<string>();
			if (Rows.Count > 0)
			{
				foreach (Column col in Rows[0].Columns)
				{
					contextValues.Add(col.Attributes[key]);
				}
			}
			return contextValues;
		}


		#region PerformCalculationOnColumn

		public void PerformCalculationOnColumn<T2>(int columnIndex, Expression<Func<T2, T2>> calculation, params int[] rowIndexes) where T2 : IComparable
		{
			if (Rows.Count > 0 && columnIndex > -1)
			{
				if (rowIndexes != null && rowIndexes.Length > 0)
				{
					for (int i = 0; i < rowIndexes.Length; i++)
					{
						Row row = Rows[rowIndexes[i]];
						object value = row.Columns[columnIndex].ActualValue;
						if (value == null)
						{
							value = default(T2);
						}
						row.Columns[columnIndex].ActualValue = calculation.Compile().Invoke((T2)value);
					}
				}
				else
				{
					foreach (Row row in Rows)
					{
						object value = row.Columns[columnIndex].ActualValue;
						if (value == null)
						{
							value = default(T2);
						}
						row.Columns[columnIndex].ActualValue = calculation.Compile().Invoke((T2)value);
					}
				}
			}
		}

		public void PerformCalculationOnColumn<T2>(string columnName, Expression<Func<T2, T2>> calculation, params int[] rowIndexes) where T2 : IComparable
		{
			int index = GetColumnIndexByName(columnName);
			PerformCalculationOnColumn(index, calculation, rowIndexes);
		}

		public void PerformCalculationOnColumn<T, T2>(Expression<Func<T, object>> columnNameExpression, Expression<Func<T2, T2>> calculation, params int[] rowIndexes) where T2 : IComparable
		{
			string columnName = Member.Of<T>(columnNameExpression).AsProperty().Name;
			PerformCalculationOnColumn(columnName, calculation, rowIndexes);
		}

		#endregion


		public bool ValidateContext(bool throwExceptions = false)
		{
			bool validated = true;
			foreach (Row row in Rows)
			{
				validated = validated & row.ValidateContext(throwExceptions);
			}
			return validated;
		}




		public static Table GetPivotedTable<T>(List<T> list, Expression<Func<T, object>>[] yAxisDefinition, Expression<Func<T, object>>[] xzAxesDefinition, PivotDefinition<T> pivotDefinition)
		{
			List<CustomDynamicObject> yAxisParts = new List<CustomDynamicObject>();
			foreach (T currentItem in list)
			{
				CustomDynamicObject yAxisPart = new CustomDynamicObject();
				foreach (Expression<Func<T, object>> yAxisPartDef in yAxisDefinition)
				{
					PropertyInfo yAxisPartDefPI = Member.Of<T>(yAxisPartDef).AsProperty();
					string name = yAxisPartDefPI.Name;
					object value = yAxisPartDefPI.GetValue(currentItem, null);
					yAxisPart.AddPropertyValue(name, value);
				}
				yAxisParts.Add(yAxisPart);
			}
			yAxisParts = yAxisParts.Distinct().ToList();

			List<CustomDynamicObject> xzAxesParts = new List<CustomDynamicObject>();
			foreach (T currentItem in list)
			{
				CustomDynamicObject xzAxesPart = new CustomDynamicObject();
				foreach (Expression<Func<T, object>> xzAxesPartDef in xzAxesDefinition)
				{
					PropertyInfo xzAxesPartDefPI = Member.Of<T>(xzAxesPartDef).AsProperty();
					string name = xzAxesPartDefPI.Name;
					object value = xzAxesPartDefPI.GetValue(currentItem, null);
					xzAxesPart.AddPropertyValue(name, value);
				}
				xzAxesParts.Add(xzAxesPart);
			}
			xzAxesParts = xzAxesParts.Distinct().ToList();


			Table workingTable = TableAndTypeConverter.GetTableFromTypedList<CustomDynamicObject>(yAxisParts);
			workingTable.IsPivoted = true;


			Dictionary<int, int> foundXAxisIndexDict = new Dictionary<int, int>();
			Dictionary<int, int> foundYAxisIndexDict = new Dictionary<int, int>();
			int foundYAxisXAxisIndex = -1;

			foreach (CustomDynamicObject currentXZ in xzAxesParts)
			{
				int foundXAxisIndex = -1;
				if (!foundXAxisIndexDict.TryGetValue(currentXZ.GetPropertyValue(pivotDefinition.xAxisInfo.Name).GetHashCode(), out foundXAxisIndex))
				{
					for (var i = 0; i < workingTable.Rows[0].Columns.Count; i++)
					{
						Column column = workingTable.Rows[0].Columns[i];
						if (column.Context.PropInfo.Name.GetHashCode() == currentXZ.GetPropertyValue(pivotDefinition.xAxisInfo.Name).GetHashCode())
						{
							foundXAxisIndex = i;
							foundXAxisIndexDict.Add(currentXZ.GetPropertyValue(pivotDefinition.xAxisInfo.Name).GetHashCode(), foundXAxisIndex);
							break;
						}
						foundXAxisIndex = -1;
					}
				}
				if (foundXAxisIndex <= -1)
				{
					workingTable.AddColumnToAllRows(currentXZ.GetPropertyValue(pivotDefinition.xAxisInfo.Name).ToString(), typeof(object) /*pivotObject.ColumnNameFinderInfo.PropertyType*/ );
					foundXAxisIndex = workingTable.Rows[0].Columns.Count - 1;
					foundXAxisIndexDict.Add(currentXZ.GetPropertyValue(pivotDefinition.xAxisInfo.Name).GetHashCode(), foundXAxisIndex);
				}

				if (foundYAxisXAxisIndex <= -1)
				{
					for (var i = 0; i < workingTable.Rows[0].Columns.Count; i++)
					{
						Column column = workingTable.Rows[0].Columns[i];
						if (column.Context.PropInfo.Name == pivotDefinition.yAxisInfo.Name)
						{
							foundYAxisXAxisIndex = i;
							break;
						}
					}
				}

				if (foundYAxisXAxisIndex > -1)
				{
					bool xyFoundAndValuePlaced = false;
					int foundYAxisIndex = -1;
					if (!foundYAxisIndexDict.TryGetValue(currentXZ.GetPropertyValue(pivotDefinition.yAxisInfo.Name).GetHashCode(), out foundYAxisIndex))
					{
						for (var i = 0; i < workingTable.Rows.Count; i++)
						{
							Row row = workingTable.Rows[i];
							if (row.Columns[foundYAxisXAxisIndex].ActualValue.CompareTo(currentXZ.GetPropertyValue(pivotDefinition.yAxisInfo.Name)) == 0)
							{
								foundYAxisIndex = i;
								foundYAxisIndexDict.Add(currentXZ.GetPropertyValue(pivotDefinition.yAxisInfo.Name).GetHashCode(), foundYAxisIndex);
								break;
							}
							foundYAxisIndex = -1;
						}
					}
					if (foundYAxisIndex > -1)
					{
						workingTable.Rows[foundYAxisIndex].Columns[foundXAxisIndex].ChangeContextType(pivotDefinition.zAxisInfo.PropertyType);
						workingTable.Rows[foundYAxisIndex].Columns[foundXAxisIndex].ActualValue = pivotDefinition.InvokeAggregateFunction(workingTable.Rows[foundYAxisIndex].Columns[foundXAxisIndex].ActualValue, (IComparable)currentXZ.GetPropertyValue(pivotDefinition.zAxisInfo.Name));
						xyFoundAndValuePlaced = true;
					}
					if (!xyFoundAndValuePlaced)
					{
						break;
						//could not find the x and y axis in which to place the value
					}
				}
				else
				{
					break;
					//the column name used to find the unique row cannot be found
				}
			}

			return workingTable;
		}
	}
}