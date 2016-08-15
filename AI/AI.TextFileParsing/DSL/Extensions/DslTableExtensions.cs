namespace AI.TextFileParsing.DSL.Extensions
{
	[MethodValuesAsStrings]
	public class DslTableExtensions
	{
		public string CurrentRow()
		{
			return "CurrentRow";
		}

		public string CurrentRowFieldIndex()
		{
			return "CurrentRow.Columns[0]";
		}

		public string StringValue()
		{
			return "StringValue";
		}

		public string ActualValue()
		{
			return "ActualValue";
		}

		public string OriginalValue()
		{
			return "OriginalValue";
		}
	}
}