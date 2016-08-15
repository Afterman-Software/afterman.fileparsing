using System;
using System.Collections.Generic;
using System.Linq;

namespace AI.TextFileParsing.ParsingDefinitions {
    using Enums;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FileType {
        public virtual int FileTypeId { get; set; }
        public virtual int FileProfileId { get; set; }
        public virtual int SortOrder { get; set; }
        public virtual string Description { get; set; }
        public virtual string SchemaIdentifier { get; set; }
        public virtual string TargetTableName { get; set; }

        public virtual FileFormatTypeEnum FileFormatType { get; set; }
        public virtual List<string> FieldDelimiters { get; set; }
        public virtual List<string> RecordDelimeters { get; set; }
        public virtual List<RowDefinition> RowDefinitions {
            get { return _rowDefinitions; }
            set { _rowDefinitions = value; }
        }
        private List<RowDefinition> _rowDefinitions = new List<RowDefinition>();

        public virtual bool IsDeleted { get; set; }
        public virtual bool IsHeader { get; set; }
        public virtual bool IsTrailer { get; set; }
        public virtual bool IsSetStart { get; set; }
        public virtual bool IsSetEnd { get; set; }
        public virtual bool IsSetAtomic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual FileType GetDeepClone() {
            var fileTypeCloned = (FileType)MemberwiseClone();
            var rowDefsCloned = new List<RowDefinition>();
            if (RowDefinitions != null) {
                rowDefsCloned.AddRange(fileTypeCloned.RowDefinitions.Select(rowDef => rowDef.GetDeepClone()));
                fileTypeCloned.RowDefinitions = rowDefsCloned;
            }
            return fileTypeCloned;
        }
    }
}
