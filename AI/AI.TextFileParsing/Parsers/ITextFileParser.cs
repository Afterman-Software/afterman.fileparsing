using System;

namespace AI.TextFileParsing.Parsers
{
	public interface ITextFileParser : IDisposable
	{
		string[] CommentTokens
		{
			get;
			set;
		}
		bool EndOfData
		{
			get;
		}
		long LineNumber
		{
			get;
		}
		string ErrorLine
		{
			get;
		}
		long ErrorLineNumber
		{
			get;
		}
		string[] ReadFields();
		string ReadLine();
		void Close();
	}
}