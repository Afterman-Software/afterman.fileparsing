using System;

namespace AI.TextFileParsing.Interfaces
{
	public interface IContext
	{
		Type DataType
		{
			get;
		}
		bool IsRequired
		{
			get;
		}
		bool HasDefaultValue
		{
			get;
		}
		string DefaultValue
		{
			get;
		}
		string FileTypeDescription
		{
			get;
		}
		int? SubstringStart
		{
			get;
		}
		int? SubstringLength
		{
			get;
		}
		string FieldFormatDescription
		{
			get;
		}
		string FieldDisplayName
		{
			get;
		}
		string FieldDescription
		{
			get;
		}
        
	}
}