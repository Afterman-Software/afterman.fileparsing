using System;

namespace AI.TextFileParsing.Generation
{
	public interface ITextFileGenerator : IDisposable
	{
		long LineNumber
		{
			get;
		}
		void WriteFields(string[] fields);
		void WriteLine();
		void Close();
	}
}