using System;
using System.Collections.Generic;
using System.Linq;

namespace AI.TextFileParsing.ParsingDefinitions
{
    [Serializable]
    public class FileProfile
    {
        public FileProfile()
        {
            FileTypes = new List<FileType>();
        }

        public virtual int FileProfileId { get; set; }
        public virtual string Description { get; set; }
        public virtual int FileTypeId { get; set; }
        public virtual string FileTypeName { get; set; }
        public virtual bool IsInboundFile { get; set; }
        public virtual bool IncludeHeaderForOutboundFile { get; set; }
        public virtual bool AppendUnmappedDataToEnd { get; set; }
        public virtual bool CaptureUnmappedDataForInboundFile { get; set; }
        public virtual bool CaptureUnmappedDataOnlyWithHeaders { get; set; }
        public virtual string Extension { get; set; }
        public virtual string Version { get; set; }
        public virtual bool IsMultiSchema { get; set; }
        public virtual bool IsFixedWidth { get; set; }
        public virtual string Delimiter { get; set; }
        public virtual string TextQualifier { get; set; }
        public virtual int? HeaderLines { get; set; }
        public virtual int? FooterLines { get; set; }
        public virtual List<FileType> FileTypes { get; set; }
        public virtual string CustomLogicDllPath { get; set; }
        public virtual string CustomLogicClassName { get; set; }
        public virtual int MaximumParsingErrors { get; set; }
        public virtual RowDefinition SchemaDetector { get; set; }
        public virtual int ClientId { get; set; }
        public virtual int Status { get; set; }
        public virtual string CustomScript { get; set; }
        public virtual string CustomAssemblyPath { get; set; }
        public virtual string CustomClassName { get; set; }
        public virtual bool IsDeleted { get; set; }

        public virtual int FileId { get; set; }
        public virtual string FileName { get; set; }

        public virtual bool ValidateRecordLengths { get; set; }

        public virtual DateTime? LastSaveDate { get; set; }


        public virtual FileProfile GetDeepClone()
        {
            FileProfile fileProfileCloned = (FileProfile)this.MemberwiseClone();
            if (this.SchemaDetector != null)
            {
                fileProfileCloned.SchemaDetector = fileProfileCloned.SchemaDetector.GetDeepClone();
            }
            if (this.FileTypes != null)
            {
                List<FileType> fileTypesCloned = new List<FileType>();
                foreach (FileType fileType in fileProfileCloned.FileTypes)
                {
                    FileType fileTypeCloned = fileType.GetDeepClone();
                    fileTypesCloned.Add(fileTypeCloned);
                }
                fileProfileCloned.FileTypes = fileTypesCloned;
            }
            return fileProfileCloned;
        }

        public virtual bool IsCustom
        {
            get
            {
                var isCustom = false;
                if (!String.IsNullOrEmpty(CustomClassName) && !String.IsNullOrEmpty(CustomAssemblyPath) && !String.IsNullOrEmpty(CustomScript))
                {
                    isCustom = true;
                }
                return isCustom;
            }
        }

        public virtual int? GetSchemaLineLength(string schemaIdentifier = null)
        {
            try
            {
                if (!this.IsMultiSchema || string.IsNullOrEmpty(schemaIdentifier))
                {
                    return this.FileTypes[0].RowDefinitions.Max(x => x.ParseStartPosition + x.ParseLength) - 1;
                }
                else
                {
                    return this.FileTypes.SingleOrDefault(x => x.SchemaIdentifier == schemaIdentifier).RowDefinitions.Max(x => x.ParseStartPosition + x.ParseLength) - 1;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}