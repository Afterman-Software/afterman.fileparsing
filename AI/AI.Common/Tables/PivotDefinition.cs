using System;
using System.Linq.Expressions;
using System.Reflection;
using AI.Common.Dynamics;

namespace AI.Common.Tables
{
	public class PivotDefinition<T>
	{
		PropertyInfo _yAxisInfo;
		PropertyInfo _xAxisInfo;
		PropertyInfo _zAxisInfo;
		Expression<Func<IComparable, IComparable, IComparable>> _aggregateFunctionExpression;
		Func<IComparable, IComparable, IComparable> _aggregateFunction;

		public PivotDefinition(Expression<Func<T, object>> yAxisExpression, Expression<Func<T, object>> xAxisExpression, Expression<Func<T, object>> zAxisExpression, Expression<Func<IComparable, IComparable, IComparable>> aggregateFunctionExpression)
		{
			_yAxisInfo = Member.Of<T>(yAxisExpression).AsProperty();
			_xAxisInfo = Member.Of<T>(xAxisExpression).AsProperty();
			_zAxisInfo = Member.Of<T>(zAxisExpression).AsProperty();
			_aggregateFunctionExpression = aggregateFunctionExpression;
			if (_aggregateFunctionExpression != null)
			{
				_aggregateFunction = _aggregateFunctionExpression.Compile();
			}
		}

		public PropertyInfo yAxisInfo
		{
			get
			{
				return _yAxisInfo;
			}
		}

		public PropertyInfo xAxisInfo
		{
			get
			{
				return _xAxisInfo;
			}
		}

		public PropertyInfo zAxisInfo
		{
			get
			{
				return _zAxisInfo;
			}
		}

		public Expression<Func<IComparable, IComparable, IComparable>> AggregateFunctionExpression
		{
			get
			{
				return _aggregateFunctionExpression;
			}
		}

		public Func<IComparable, IComparable, IComparable> AggregateFunction
		{
			get
			{
				return _aggregateFunction;
			}
		}

		public IComparable InvokeAggregateFunction(IComparable arg1, IComparable arg2)
		{
			if (_aggregateFunction != null)
			{
				return _aggregateFunction.Invoke(arg1, arg2);
			}
			return arg1;
		}
	}
}