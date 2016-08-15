using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AI.TextFileParsing.Generation
{
	public class FixedWidthTextFileGenerator : ITextFileGenerator
	{
		private readonly StreamWriter _writer;
		private long _lineCounter;

		public FixedWidthTextFileGenerator(StreamWriter streamWriter)
		{
		    _writer = streamWriter;
		}

		public long LineNumber
		{
			get { return _lineCounter; }
		}

		public string Delimiter
		{
			get
			{
				return null;
			}
			set
			{
				//no-op
			}
		}

		public string TextQualifier
		{
			get
			{
				return null;
			}
			set
			{
				//no-op
			}
		}

		public void WriteFields(string[] fields)
		{
			string fieldsLine = string.Join(string.Empty, fields);
			_writer.WriteLine(fieldsLine);
			_lineCounter++;
		}

		public void WriteLine()
		{
			_writer.WriteLine();
			_lineCounter++;
		}

		public void Close()
		{
			_writer.Close();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				try
				{
					if (_writer != null)
					{
						_writer.Close();
						_writer.Dispose();
					}
				}
				catch
				{
					//swallow exceptions and just dispose
				}
			}
		}
	}
}