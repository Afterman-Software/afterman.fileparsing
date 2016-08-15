using System.IO;
using AI.TextFileParsing.ParsingDefinitions;

namespace AI.TextFileParsing.Parsers
{
	public class ParserFactory
	{
		private static object _lock = new object();
		private static ParserFactory _instance;

		private ParserFactory()
		{

		}

		public static ParserFactory Instance
		{

			get
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = new ParserFactory();
					}
				}
				return _instance;
			}
		}

		public T GetParser<T>(FileProfile fileProfile, Stream stream)
			where T : class, ITextFileParser
		{
			return GetParser(fileProfile, stream) as T;
		}

		public T GetSchemaParser<T>(FileProfile fileProfile, Stream stream)
			where T : class, ITextFileParser
		{
			return GetSchemaParser(fileProfile, stream) as T;
		}

		public ITextFileParser GetSchemaParser(FileProfile fileProfile, Stream stream)
		{
			if (fileProfile.IsFixedWidth)
			{
				var parser = new FixedWidthTextFileParser(stream);
				if (fileProfile.SchemaDetector.ParseStartPosition.HasValue && fileProfile.SchemaDetector.ParseStartPosition.Value > 0 &&
					fileProfile.SchemaDetector.ParseLength.HasValue && fileProfile.SchemaDetector.ParseLength.Value > 0)
				{
					parser.AddFieldWidth(fileProfile.SchemaDetector.ParseStartPosition.Value, fileProfile.SchemaDetector.ParseLength.Value);
					fileProfile.SchemaDetector.SourceColumnNumber = 1;
				}
				return parser;
			}
			else
			{
				var parser = new DelimitedTextFileParser(stream);
				parser.Delimiters = new[] { fileProfile.Delimiter };
				if (!string.IsNullOrWhiteSpace(fileProfile.TextQualifier))
				{
					parser.HasFieldsEnclosedInQualifier = true;
					parser.SetTextQualifier(fileProfile.TextQualifier);
				}
				else
				{
					parser.HasFieldsEnclosedInQualifier = false;
				}
				return parser;
			}
		}

		public ITextFileParser GetParser(FileProfile fileProfile, Stream stream)
		{
			if (fileProfile.IsFixedWidth)
			{
				var parser = new FixedWidthTextFileParser(stream);
				int sourceColNum = 1;
				foreach (RowDefinition rowDefinition in fileProfile.FileTypes[0].RowDefinitions)
				{
					if (rowDefinition.ParseStartPosition.HasValue && rowDefinition.ParseStartPosition.Value > 0 &&
						rowDefinition.ParseLength.HasValue && rowDefinition.ParseLength.Value > 0)
					{
						parser.AddFieldWidth(
							rowDefinition.ParseStartPosition.Value,
							rowDefinition.ParseLength.Value);
						rowDefinition.SourceColumnNumber = sourceColNum;
						sourceColNum++;
					}
					else
					{
						rowDefinition.SourceColumnNumber = null;
					}
				}
				return parser;
			}
			else
			{
				var parser = new DelimitedTextFileParser(stream);
				parser.Delimiters = new[] { fileProfile.Delimiter };
				if (!string.IsNullOrWhiteSpace(fileProfile.TextQualifier))
				{
					parser.HasFieldsEnclosedInQualifier = true;
					parser.SetTextQualifier(fileProfile.TextQualifier);
				}
				else
				{
					parser.HasFieldsEnclosedInQualifier = false;
				}
				return parser;
			}
		}
	}
}
