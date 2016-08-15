using System;
using System.Collections.Generic;
using AI.TextFileParsing.Enums;

namespace AI.TextFileParsing.ParsingDefinitions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ParseTargetAttribute : Attribute
    {
        public ParseTargetAttribute(string displayName)
        {
            this.DisplayName = displayName;
            this.FileTypes = new List<string>();
        }

        public ParseTargetAttribute(string displayName, params string[] fileTypes)
        {
            this.DisplayName = displayName;
            this.FileTypes = new List<string>(fileTypes);
        }

        public IList<string> FileTypes { get; set; }

        public string DisplayName { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ParseTargetFieldAttribute : Attribute
    {
        public ParseTargetFieldAttribute(string displayName, DataTypeEnum dataType)
        {
            this.DataType = dataType;
            this.DisplayName = displayName;
        }

        public string DisplayName { get; private set; }

        public DataTypeEnum DataType { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class IgnoreParseTargetFieldAttribute : Attribute
    {
        public IgnoreParseTargetFieldAttribute()
        {

        }


    }

}
