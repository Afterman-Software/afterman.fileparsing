using System;
using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing.ParseException
{
    [Serializable]
    public class ParseConversionException : Exception
    {
        public ParseConversionException(IContext context, string workingStringValue, Exception innerException)
            : base("Exception encountered while parsing file", innerException)
        {
            this.WorkingStringValue = workingStringValue;
            this.DataType = context.DataType;
            this.IsRequired = context.IsRequired;
            this.HasDefaultValue = context.HasDefaultValue;
            this.DefaultValue = context.DefaultValue;
            this.FileTypeDescription = context.FileTypeDescription;
            this.SubstringStart = context.SubstringStart;
            this.SubstringLength = context.SubstringLength;
            this.FieldFormatDescription = context.FieldFormatDescription;
            this.FieldDisplayName = context.FieldDisplayName;
            this.FieldDescription = context.FieldDescription;
        }

        public string WorkingStringValue { get; private set; }

        public Type DataType { get; private set; }

        public bool IsRequired { get; private set; }

        public bool HasDefaultValue { get; private set; }

        public string DefaultValue { get; private set; }

        public string FileTypeDescription { get; private set; }

        public int? SubstringStart { get; private set; }

        public int? SubstringLength { get; private set; }

        public string FieldFormatDescription { get; private set; }

        public string FieldDisplayName { get; private set; }

        public string FieldDescription { get; private set; }
    }
}
