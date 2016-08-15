namespace AI.TextFileParsing.Interfaces
{
	public interface IColumn
	{
		string OriginalValue
		{
			get;
		}
		string StringValue
		{
			get;
			set;
		}
		IContext Context
		{
			get;
		}
		object ActualValue
		{
			get;
			set;
		}
	}
}