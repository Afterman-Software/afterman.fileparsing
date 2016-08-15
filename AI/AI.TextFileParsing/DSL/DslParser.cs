using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing.DSL
{
	public abstract class DslParser : IDslScript
	{

		public abstract object CalculateValue();

		#region accessors
		public IColumn Column { get; set; }

		public string StringValue
		{
			get
			{
				return Column.StringValue;
			}
		}

		public IRow CurrentRow { get; set; }

		public ITable Table { get; set; }


		public object ActualValue
		{
			get { return Column.ActualValue; }
			protected set { Column.ActualValue = value; }
		}

		public object OriginalValue
		{

			get { return Column.OriginalValue; }
		}
		#endregion

		public void Execute()
		{
			this.ActualValue = CalculateValue();
		}




		public Extensions.DslStringExtensions Text
		{
			get { return new Extensions.DslStringExtensions(); }
		}

		public Extensions.DslMathExtensions Math
		{
			get { return new Extensions.DslMathExtensions(); }
		}

		public Extensions.DslCommonExtensions Common
		{
			get { return new Extensions.DslCommonExtensions(); }
		}

		public Extensions.DslTableExtensions Tables
		{
			get { return new Extensions.DslTableExtensions(); }
		}
	}
}
