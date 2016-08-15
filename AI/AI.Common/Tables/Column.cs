using System;
using System.Collections.Generic;
using AI.Common.Dynamics;

namespace AI.Common.Tables
{
    [Serializable]
	public class Column
	{
		private Row _parentRow;

		private Context _context;
		private Dictionary<string, string> _attributes = new Dictionary<string, string>();
		private IComparable _actualValue;

		protected internal Column
			(
				Row parentRow,
				IComparable actualValue,
				Context context
			)
		{
			_parentRow = parentRow;
			_context = context;
			if (ValidateContext(actualValue, false))
				_actualValue = actualValue;
		}

		public Table ParentTable
		{
			get
			{
				return _parentRow.ParentTable;
			}
		}

		public Row ParentRow
		{
			get
			{
				return _parentRow;
			}
		}

		public Dictionary<string, string> Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public Context Context
		{
			get
			{
				return _context;
			}
		}

		public void ChangeContextType(Type newType)
		{
			CreateablePropertyInfo cpi = new CreateablePropertyInfo(_context.PropInfo.Name, newType);
			_context = new Context(cpi.ToPropertyInfo());
		}

		public IComparable ActualValue
		{
			get
			{
				return _actualValue;
			}
			set
			{
				if (ValidateContext(value, false))
					_actualValue = value;
			}
		}

		public bool ValidateContext(object value, bool throwExceptions = true)
		{
			if (!_context.PropInfo.CanWrite)
			{
				if (throwExceptions)
				{
					throw new MemberAccessException("The field, " + _context.PropInfo.Name + ", is read-only.");
				}
				else
				{
					return false;
				}
			}
			if (value == null && !_context.IsNullableType())
			{
				if (throwExceptions)
				{
					throw new ArgumentNullException("value", "The field, " + _context.PropInfo.Name + ", is required and cannot be null.");
				}
				else
				{
					return false;
				}
			}
			if (value != null && ((_context.IsNullableType() && Nullable.GetUnderlyingType(_context.PropInfo.PropertyType) != value.GetType()) && (!_context.IsNullableType() && _context.PropInfo.PropertyType != value.GetType())))
			{
				if (throwExceptions)
				{
					throw new InvalidCastException("The field, " + _context.PropInfo.Name + ", must be of type " + _context.PropInfo.PropertyType + " but is of type " + value.GetType().Name + ".");
				}
				else
				{
					return false;
				}
			}
			return true;
		}
	}
}