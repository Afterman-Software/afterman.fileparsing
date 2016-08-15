using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AI.Common.Dynamics;
using AI.Common.Extensions.Sys;
using AI.Common.Security;
using AI.TextFileParsing.Enums;
using AI.TextFileParsing.Generation;
using AI.TextFileParsing.ParsingDefinitions;

namespace AI.TextFileParsing
{
    public class TextFileGenerator : IDisposable
    {
        private readonly FileProfile _fileToGenerate;

        private ITextFileGenerator _generator;

        private long _currentLineNumber;

        private int _headerLines;
        private int _footerLines;

        private SecureFileData fileData = null;
        string errorType = string.Empty;

        public TextFileGenerator(FileProfile fileToGenerate)
        {
            _fileToGenerate = fileToGenerate;
        }

        public void StartGenerate()
        {
            fileData = new SecureFileData(SharedTempFile.NewFile());
            fileData.OriginalFileName = _fileToGenerate.FileName;

            var streamWriter = fileData.GetStreamWriter();

            try
            {
                errorType = string.Empty;

                _headerLines = (_fileToGenerate.HeaderLines.HasValue && _fileToGenerate.HeaderLines.Value > 0 ? _fileToGenerate.HeaderLines.Value : 0);
                _footerLines = (_fileToGenerate.FooterLines.HasValue && _fileToGenerate.FooterLines.Value > 0 ? _fileToGenerate.FooterLines.Value : 0);

                if (_fileToGenerate.IsFixedWidth)
                {
                    _generator = new FixedWidthTextFileGenerator(streamWriter);
                    int sourceColNum = 1;
                    if (_fileToGenerate.IsMultiSchema)
                    {
                        foreach (FileType fileType in _fileToGenerate.FileTypes)
                        {
                            sourceColNum = 1;
                            foreach (RowDefinition rowDefinition in fileType.RowDefinitions)
                            {
                                if (rowDefinition.ParseStartPosition.HasValue && rowDefinition.ParseStartPosition.Value > 0 &&
                                    rowDefinition.ParseLength.HasValue && rowDefinition.ParseLength.Value > 0)
                                {
                                    rowDefinition.SourceColumnNumber = sourceColNum;
                                    sourceColNum++;
                                }
                                else
                                {
                                    rowDefinition.SourceColumnNumber = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (RowDefinition rowDefinition in _fileToGenerate.FileTypes[0].RowDefinitions)
                        {
                            if (rowDefinition.ParseStartPosition.HasValue && rowDefinition.ParseStartPosition.Value > 0 &&
                                rowDefinition.ParseLength.HasValue && rowDefinition.ParseLength.Value > 0)
                            {
                                rowDefinition.SourceColumnNumber = sourceColNum;
                                sourceColNum++;
                            }
                            else
                            {
                                rowDefinition.SourceColumnNumber = null;
                            }
                        }
                    }
                }
                else
                {
                    _generator = new DelimitedTextFileGenerator(streamWriter);

                    ((DelimitedTextFileGenerator)_generator).Delimiter = _fileToGenerate.Delimiter;
                    ((DelimitedTextFileGenerator)_generator).TextQualifier = _fileToGenerate.TextQualifier;
                }

                string checkRowsErrors = string.Empty;
                if (_fileToGenerate.IsMultiSchema)
                {
                    foreach (FileType checkFileType in _fileToGenerate.FileTypes)
                    {
                        foreach (RowDefinition checkRowDef in checkFileType.RowDefinitions)
                        {
                            if (checkRowDef.IsRequired &&
                                (!checkRowDef.SourceColumnNumber.HasValue || checkRowDef.SourceColumnNumber.Value <= 0))
                            {
                                checkRowsErrors += checkRowDef.FieldDisplayName + " is a required field but has no definition in the profile." +
                                                   Environment.NewLine;
                            }
                        }
                    }
                }
                else
                {
                    foreach (RowDefinition checkRowDef in _fileToGenerate.FileTypes[0].RowDefinitions)
                    {
                        if (checkRowDef.IsRequired &&
                            (!checkRowDef.SourceColumnNumber.HasValue || checkRowDef.SourceColumnNumber.Value <= 0))
                        {
                            checkRowsErrors += checkRowDef.FieldDisplayName + " is a required field but has no definition in the profile." +
                                               Environment.NewLine;
                        }
                    }
                }
                checkRowsErrors = checkRowsErrors.Trim();
                if (!string.IsNullOrWhiteSpace(checkRowsErrors))
                {
                    throw new Exception(checkRowsErrors);
                }

                if (_fileToGenerate.IsMultiSchema)
                {
                    RowDefinition schemaDet = _fileToGenerate.SchemaDetector;
                    schemaDet.IsRequired = false;
                    schemaDet.SourceColumnNumber = -1;
                    foreach (FileType checkFileType in _fileToGenerate.FileTypes)
                    {
                        List<RowDefinition> rowDefinitions = checkFileType.RowDefinitions.ToList();
                        rowDefinitions.Add(schemaDet);
                        rowDefinitions = rowDefinitions.OrderBy(x => x.SourceColumnNumber).ToList();
                        checkFileType.RowDefinitions = rowDefinitions;
                        foreach (RowDefinition checkRowDef in checkFileType.RowDefinitions)
                        {
                            if (checkRowDef.IsRequired &&
                                (!checkRowDef.SourceColumnNumber.HasValue || checkRowDef.SourceColumnNumber.Value <= 0))
                            {
                                checkRowsErrors += checkRowDef.FieldDisplayName + " is a required field but has no definition in the profile." +
                                                   Environment.NewLine;
                                throw new Exception(checkRowsErrors);
                            }
                        }
                    }
                }

                //skip header lines if defined
                if (_headerLines > 0)
                {
                    short skipHeaderLines = 0;
                    while (skipHeaderLines < _headerLines)
                    {
                        _generator.WriteLine();
                        skipHeaderLines++;
                        _currentLineNumber++;
                    }
                }
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(errorType))
                    throw new Exception(errorType + ": " + e.Message);
                throw;
            }
        }

        public void GenerateLine(Tuple<object, List<object>> dataObjects, List<Exception> exceptions)
        {
            bool isFixedWidth = _fileToGenerate.IsFixedWidth;
            bool isMultiSchema = _fileToGenerate.IsMultiSchema;
            errorType = "File generation error";
            {
                FileType fileType = null;
                if (isMultiSchema)
                {
                    fileType = _fileToGenerate.FileTypes.SingleOrDefault(ft => ft.SchemaIdentifier == (string)dataObjects.Item1);
                }
                else
                {
                    fileType = _fileToGenerate.FileTypes[0];
                }
                if (fileType != null)
                {
                    RowDefinition rowDef = null;
                    try
                    {
                        int lastPos = 0;
                        if (isFixedWidth)
                            lastPos = 1;
                        ArrayList fieldList = new ArrayList();
                        List<RowDefinition> rowDefs = fileType.RowDefinitions.OrderBy(rd => rd.SourceColumnNumber).ToList();
                        for (var i = 0; i < rowDefs.Count; i++)
                        {
                            rowDef = rowDefs[i];
                            if (isFixedWidth)
                            {
                                if (rowDef.ParseStartPosition > lastPos)
                                {
                                    int len = rowDef.ParseStartPosition.Value - lastPos;
                                    var padding = new string(' ', len);
                                    fieldList.Add(padding);
                                }
                                lastPos = rowDef.ParseStartPosition.Value + rowDef.ParseLength.Value;
                            }
                            else
                            {
                                if (rowDef.SourceColumnNumber == null)
                                    continue;

                                if (rowDef.SourceColumnNumber > (lastPos + 1))
                                {
                                    for (var s = 0; s < rowDef.SourceColumnNumber - 1; s++)
                                    {
                                        fieldList.Add("");
                                    }
                                }
                                lastPos = rowDef.SourceColumnNumber.Value;
                            }
                            object obj = null;
                            if (rowDef.SourceColumnNumber == -1)
                            {
                                string schemaId = ConvertValue(fileType.SchemaIdentifier, rowDef.DataType, rowDef.FieldFormat, isFixedWidth, rowDef.ParseLength);
                                if (schemaId != null)
                                    fieldList.Add(schemaId);
                            }
                            else
                            {
                                obj = dataObjects.Item2.SingleOrDefault(o => o.GetType().Name == rowDef.TargetTableName);
                                if (obj == null)
                                {
                                    obj = dataObjects.Item2.SingleOrDefault(o => o.GetType().GetCustomAttribute<ParseTargetAttribute>() != null && o.GetType().GetCustomAttribute<ParseTargetAttribute>().DisplayName == rowDef.TargetTableName);
                                }
                            }
                            if (obj != null)
                            {
                                PropertyInfo pInfo = obj.GetType().GetProperty(rowDef.TargetFieldName);
                                if (pInfo != null)
                                {
                                    string result = null;
                                    object value = pInfo.GetValue(obj);
                                    if (value == null && rowDef.IsRequired)
                                    {
                                        if (rowDef.CanSetDefault && rowDef.DefaultValue != null)
                                        {
                                            value = rowDef.DefaultValue;
                                        }
                                        if (value == null)
                                        {
                                            throw new Exception("[" + rowDef.FieldDisplayName + "] is a required field but has no value.");
                                        }
                                    }

                                    result = ConvertValue(value, rowDef.DataType, rowDef.FieldFormat, isFixedWidth, rowDef.ParseLength);

                                    if (result != null)
                                        fieldList.Add(result);
                                    else
                                        fieldList.Add("");
                                }
                            }
                        }
                        if (!_fileToGenerate.IsMultiSchema && !_fileToGenerate.IsFixedWidth && _fileToGenerate.AppendUnmappedDataToEnd)
                        {
                            CustomDynamicObject cdo = null;
                            foreach (object obj in dataObjects.Item2)
                            {
                                List<PropertyInfo> propList = TypedPropertyList.GetPropertyList(obj.GetType(), false, true);
                                foreach (PropertyInfo prop in propList)
                                {
                                    object propVal = prop.GetValue(obj);
                                    if (propVal is CustomDynamicObject)
                                    {
                                        cdo = propVal as CustomDynamicObject;
                                        break;
                                    }
                                    if (cdo != null)
                                        break;
                                }
                            }
                            if (cdo != null)
                            {
                                var dictNamesTypes = cdo.GetPropertyNamesAndTypes();
                                foreach (var dictNameType in dictNamesTypes)
                                {
                                    var val = cdo.GetPropertyValue(dictNameType.Key);
                                    fieldList.Add(val != null ? val.ToString() : "");
                                }
                            }
                        }
                        _generator.WriteFields(fieldList.Cast<string>().ToArray());
                    }
                    catch (Exception e)
                    {
                        string columnDesc = string.Empty;
                        if (rowDef != null)
                            columnDesc = string.Format("Field '{0}', ", rowDef.FieldDisplayName);

                        exceptions.Add(new Exception("Generation Warning",
                                                     new Exception(string.Format(
                                                         "Unable to generate line {0}, {1} column {2}.  Error: {3}", _currentLineNumber + 1,
                                                         columnDesc, rowDef.SourceColumnNumber.Value, e.Message))));
                        if (_fileToGenerate.MaximumParsingErrors > 0 && exceptions.Count >= _fileToGenerate.MaximumParsingErrors)
                        {
                            errorType = string.Empty;
                            throw new Exception(string.Format("Aborting export after {0} Generation Warnings",
                                                              _fileToGenerate.MaximumParsingErrors));
                        }
                    }
                }
                _currentLineNumber++;
            }

            //skip footer lines if defined
            if (_footerLines > 0)
            {
                short skipFooterLines = 0;
                while (skipFooterLines < _footerLines)
                {
                    _generator.WriteLine();
                    skipFooterLines++;
                    _currentLineNumber++;
                }
            }
        }

        public void WriteFields(string[] fields)
        {
            _generator.WriteFields(fields);
        }

        public SecureFileData EndGenerate()
        {
            _generator.Close();

            return fileData;
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
                    if (_generator != null)
                        _generator.Dispose();
                }
                catch
                {
                    //swallow exceptions and just dispose
                }
            }
        }

        internal string ConvertValue(object value, int dataTypeId, int fieldFormatId, bool isFixedWidth, int? parseLength)
        {
            if (null == value)
                return null;
            string result = null;
            switch (dataTypeId)
            {
                case (int)DataTypeEnum.String:
                    result = string.IsNullOrEmpty((string)value) ? "" : (string)value;
                    if (isFixedWidth)
                    {
                        result = result.ToFixedLength(parseLength.Value);
                    }
                    break;
                case (int)DataTypeEnum.Integer:
                    result = ((int)value).ToString();
                    if (isFixedWidth)
                    {
                        result = result.ToFixedLength(parseLength.Value, '0', true);
                    }
                    break;
                case (int)DataTypeEnum.DecimalCurrency:
                    decimal workingCurrency = (decimal)value;
                    result = ConvertCurrency(workingCurrency, fieldFormatId);
                    if (isFixedWidth)
                    {
                        result = result.ToFixedLength(parseLength.Value, ' ', true);
                    }
                    break;
                case (int)DataTypeEnum.DecimalPercent:
                    decimal workingPercent = (decimal)value;
                    result = ConvertRate(workingPercent, fieldFormatId);
                    if (isFixedWidth)
                    {
                        result = result.ToFixedLength(parseLength.Value, ' ', true);
                    }
                    break;
                case (int)DataTypeEnum.Date:
                    DateTime workingDate = (DateTime)value;
                    result = ConvertDate(workingDate, fieldFormatId, parseLength);
                    if (isFixedWidth)
                    {
                        result = result.ToFixedLength(parseLength.Value, ' ', true);
                    }
                    break;
                case (int)DataTypeEnum.Time:
                    DateTime workingTime = (DateTime)value;
                    result = ConvertTime(workingTime, fieldFormatId);
                    if (isFixedWidth)
                    {
                        result = result.ToFixedLength(parseLength.Value, ' ', true);
                    }
                    break;
            }
            return result;
        }

        internal string ConvertDate(DateTime? value, int? fieldFormatId, int? length)
        {
            if (!fieldFormatId.HasValue)
                return length.HasValue ? new string(' ', length.Value) : "";

            if (!value.HasValue)
                return length.HasValue ? new string(' ', length.Value) : "";

            DateFormatEnum patternEnum = (DateFormatEnum)fieldFormatId;
            string pattern = patternEnum.GetDisplayDescription();

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            switch (pattern)
            {
                case "MM/dd/yyyy":
                case "MM/dd/yy":
                case "MM-dd-yy":
                case "MM-dd-yyyy":
                case "MMM-dd-yy":
                case "MMM-dd-yyyy":
                case "MMM-yy":
                case "MMM-yyyy":
                case "M/d/yyyy":
                case "yyyy-MM-dd":
                case "yyMMdd":
                case "ddMMyy":
                case "MMddyyyy":
                case "yyddMM":
                case "yyyyddMM":
                case "yyyyMMdd":
                case "dd-MMM-yy":
                case "MMddyy":
                case "Mddyy":
                case "Mddyyyy":
                case "#yyyy-MM-dd#":
                    return value.Value.ToString(pattern, cultureInfo);
                case "MMddyy/Mddyy":
                case "MMddyyyy/Mddyyyy":
                    if (length.HasValue && length.Value == 5)
                    {
                        return value.Value.ToString("Mddyy", cultureInfo);
                    }
                    if (length.HasValue && length.Value == 6)
                    {
                        return value.Value.ToString("MMddyy", cultureInfo);
                    }
                    if (length.HasValue && length.Value == 7)
                    {
                        return value.Value.ToString("Mddyyyy", cultureInfo);
                    }
                    if (length.HasValue && length.Value == 8)
                    {
                        return value.Value.ToString("MMddyyyy", cultureInfo);
                    }
                    return length.HasValue ? new string(' ', length.Value) : "";
                case "yyyyJJJ":
                    return value.Value.Year.ToString("0000", cultureInfo) + value.Value.DayOfYear.ToString("000", cultureInfo);
                case "yyJJJ":
                    return value.Value.Year.ToString("0000", cultureInfo).Substring(2) + value.Value.DayOfYear.ToString("000", cultureInfo);
                case "JJJyy":
                    return value.Value.DayOfYear.ToString("000", cultureInfo) + value.Value.Year.ToString("0000", cultureInfo).Substring(2);
                case "JJJyyyy":
                    return value.Value.DayOfYear.ToString("000", cultureInfo) + value.Value.Year.ToString("0000", cultureInfo);
                default:
                    return length.HasValue ? new string(' ', length.Value) : "";
            }
        }

        internal string ConvertTime(DateTime? value, int? fieldFormatId)
        {
            if (!fieldFormatId.HasValue)
                return "";

            if (!value.HasValue)
                return "";

            TimeFormatEnum patternEnum = (TimeFormatEnum)fieldFormatId;
            string pattern = patternEnum.GetDisplayDescription();

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            switch (pattern)
            {
                case "Unknown Format":
                    return "";
                default:
                    return value.Value.ToString(pattern, cultureInfo);
            }
        }

        internal string ConvertRate(decimal? value, int? fieldFormatId)
        {
            if (!fieldFormatId.HasValue)
                return "";

            if (!value.HasValue)
                return "";

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            switch (fieldFormatId.Value)
            {
                case 0:
                    return value.Value.ToString("0.000%", cultureInfo);
                case 1:
                    return (value.Value * 100M).ToString("0.0000", cultureInfo);
                case 2:
                    return (value.Value).ToString(".00000", cultureInfo);
                case 3:
                    return "%" + (value.Value * 100M).ToString("0.000", cultureInfo);
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    //TODO: How to handle this
                    return value.Value.ToString("0.000", cultureInfo);
            }

            return "";
        }

        internal string ConvertCurrency(decimal? value, int? fieldFormatId)
        {
            if (!fieldFormatId.HasValue)
                return "";

            if (!value.HasValue)
                return "";

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            switch (fieldFormatId.Value)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    //TODO: How to handle this
                    return value.Value.ToString("0.000", cultureInfo);
            }

            return "";
        }
    }
}