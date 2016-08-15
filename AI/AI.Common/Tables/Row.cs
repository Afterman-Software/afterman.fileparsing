using System;
using System.Collections.Generic;

namespace AI.Common.Tables
{
    [Serializable]
	public class Row
	{
		private Table _parentTable;

		private string _id = null;

		private Dictionary<string, string> _attributes = new Dictionary<string, string>();

		private List<Column> _columns;

		protected internal Row(Table parentTable)
		{
			_parentTable = parentTable;
			_columns = new List<Column>();
		}

		public Table ParentTable
		{
			get
			{
				return _parentTable;
			}
		}

		public string Id
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public Dictionary<string, string> Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public List<Column> Columns
		{
			get
			{
				return _columns;
			}
		}

		public bool ValidateContext(bool throwExceptions = false)
		{
			bool validated = true;
			foreach (Column column in Columns)
			{
				validated = validated & column.ValidateContext(column.ActualValue, throwExceptions);
			}
			return validated;
		}
	}
}