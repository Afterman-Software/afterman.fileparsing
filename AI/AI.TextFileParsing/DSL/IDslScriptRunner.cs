using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing.DSL
{
	public interface IDslScriptRunner
	{
		void RunScript(string assembly, string className, IColumn column, IRow row, ITable table);
	}
}