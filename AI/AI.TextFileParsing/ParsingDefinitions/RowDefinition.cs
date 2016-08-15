using System;

namespace AI.TextFileParsing.ParsingDefinitions
{
    [Serializable]
    public class RowDefinition
    {
        public virtual int FieldDictionaryId { get; set; }
        public virtual int? FileTypeId { get; set; }
        public virtual string FieldDisplayName { get; set; }
        public virtual string TargetTableName { get; set; }
        public virtual string TargetFieldName { get; set; }
        public virtual int DataType { get; set; }
        public virtual string DataTypeDescription { get; set; }
        public virtual bool IsRequired { get; set; }
        public virtual bool CanSetDefault { get; set; }
        public virtual int SortOrder { get; set; }
        public virtual bool IsSuppressed { get; set; }
        public virtual int? SourceColumnNumber { get; set; }
        public virtual int? ParseStartPosition { get; set; }
        public virtual int? ParseLength { get; set; }
        public virtual int FieldFormat { get; set; }
        public virtual string FieldFormatDescription { get; set; }
        public virtual string DefaultValue { get; set; }
        public virtual string FieldDescription { get; set; }
        public virtual int? FileProfileId { get; set; }
        public virtual string CustomScript { get; set; }
        public virtual string CustomAssemblyPath { get; set; }
        public virtual string CustomClassName { get; set; }
        public virtual string AIScriptSource { get; set; }
        public virtual bool GroupByThisColumn { get; set; }

        private string _targetFieldDisplayName;

        public virtual string TargetFieldDisplayName
        {
            get
            {
                if (String.IsNullOrEmpty(_targetFieldDisplayName))
                    return TargetFieldName;
                return _targetFieldDisplayName;
            }
            set { _targetFieldDisplayName = value; }
        }



        public virtual bool IsDeleted { get; set; }

        public virtual RowDefinition GetDeepClone()
        {
            return (RowDefinition)this.MemberwiseClone();
        }

        public virtual int IsCustom
        {
            get
            {
                if (!String.IsNullOrEmpty(AIScriptSource))
                    return 1;

                if (!String.IsNullOrEmpty(CustomClassName) && !String.IsNullOrEmpty(CustomAssemblyPath) && !String.IsNullOrEmpty(CustomScript))
                    return 2;

                return 0;
            }
        }
    }
}