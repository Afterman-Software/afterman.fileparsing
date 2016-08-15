using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AI.Common.Extensions.Sys;
using AI.Common.Security;
using AI.TextFileParsing.DSL;
using AI.TextFileParsing.Enums;
using AI.TextFileParsing.Interfaces;
using AI.TextFileParsing.Parsers;
using AI.TextFileParsing.ParsingDefinitions;
using AI.TextFileParsing.Scripting;
using AI.TextFileParsing.Services;

namespace AI.TextFileParsing
{
    public class TextFileParser : IDisposable
    {
        private readonly FileProfile _fileToProcess;

        private Stream _fileStream;
        private ITextFileParser _parser;
        private Table _table;
        private readonly IDslScriptRunner _scriptRunner;
        private readonly IFileToStreamConverter _fileToStreamConverter;
        private long _totalLines;
        private long _currentLineNumber;
        private int _headerLines;
        private int _footerLines;
        private Dictionary<string, Context> _contextLookup = new Dictionary<string, Context>();
        private readonly IFileData _fileData;
        private string[] _headerFields = null;

        public TextFileParser(FileProfile fileToProcess, IFileData fileData, IDslScriptRunner scriptRunner, string[] headerFields, int startAtLineNumber = -1)
        {
            _table = new Table();
            _scriptRunner = scriptRunner;
            _fileToStreamConverter = new FileToStreamConverter();
            _fileToProcess = fileToProcess;
            _fileData = fileData;
            _headerFields = headerFields;
            _headerLines = startAtLineNumber;
        }

        public Table Parse(List<Exception> exceptions)
        {
            _fileStream = _fileData.GetReadStream();
            _totalLines = _fileData.GetLineCount();
            if (_headerLines < 0)
            {
                _headerLines = (_fileToProcess.HeaderLines.HasValue && _fileToProcess.HeaderLines.Value > 0 ? _fileToProcess.HeaderLines.Value : 0);
            }
            _footerLines = (_fileToProcess.FooterLines.HasValue && _fileToProcess.FooterLines.Value > 0 ? _fileToProcess.FooterLines.Value : 0);
            ParseSingleSchemaFile(exceptions);
            //File Profile level scripting should not be supported because it will enumerate/yield all records, causing downstream failures
            //if (_fileToProcess.IsCustom)
            //{
            //    _scriptRunner.RunScript(_fileToProcess.CustomAssemblyPath, _fileToProcess.CustomClassName, null, null, _table);
            //}
            return _table;
        }

        private void ParseSingleSchemaFile(List<Exception> exceptions)
        {
            _parser = ParserFactory.Instance.GetParser(_fileToProcess, _fileStream);
            ValidateProfileDefinition(null);
            SkipHeaderLines();
            Dictionary<RowDefinition, IColumn> scriptRows = new Dictionary<RowDefinition, IColumn>();
            _table.Rows = GetSingleSchemaRows(exceptions, scriptRows, _table);
            //File Profile level scripting should not be supported because it will enumerate/yield all records, causing downstream failures
            //if (_fileToProcess.IsCustom)
            //{
            //    _scriptRunner.RunScript(_fileToProcess.CustomAssemblyPath, _fileToProcess.CustomClassName, null, null, _table);
            //}
        }

        private IEnumerable<IRow> GetSingleSchemaRows(List<Exception> exceptions, Dictionary<RowDefinition, IColumn> scriptRows, ITable table)
        {
            var errorType = "File parsing error";

            while (!_parser.EndOfData)
            {
                IRow row = new Row();
                RowDefinition rowDef = null;
                RowDefinitionScriptConfiguration rdsc = null;
                try
                {
                    if (_footerLines > 0 && _currentLineNumber > _totalLines - _footerLines)
                    {
                        break;
                    }

                    string[] fields = _parser.ReadFields();
                    if (fields == null || fields.Length == 0)
                        continue;
                    List<int> usedFieldIndexes = new List<int>();

                    foreach (var rowDefinition in _fileToProcess.FileTypes[0].RowDefinitions)
                    {
                        rowDef = rowDefinition;
                        if (rowDef.SourceColumnNumber.HasValue && rowDef.SourceColumnNumber.Value > 0)
                        {
                            IContext context = null;

                            var contextKey = _fileToProcess.FileTypes[0].FileTypeId + "-" + rowDef.SourceColumnNumber.Value + ":" + rowDef.TargetTableName + "." + rowDef.TargetFieldName;

                            if (!this._contextLookup.ContainsKey(_fileToProcess.FileTypes[0].FileTypeId + "-" + rowDef.SourceColumnNumber.Value))
                            {
                                _contextLookup[contextKey] = new Context((DataTypeEnum)rowDef.DataType, rowDef.IsRequired, rowDef.CanSetDefault,
                                                           rowDef.DefaultValue, _fileToProcess.FileTypes[0].Description,
                                                           (_fileToProcess.IsFixedWidth ? null : rowDef.ParseStartPosition),
                                                           (_fileToProcess.IsFixedWidth ? null : rowDef.ParseLength), rowDef.FieldFormat,
                                                           rowDef.FieldDisplayName, rowDef.FieldDescription, rowDef.TargetTableName,
                                                           rowDef.TargetFieldName);
                            }

                            context = this._contextLookup[contextKey];
                            string fieldValue = fields[rowDef.SourceColumnNumber.Value - 1];
                            if (fieldValue != null && fieldValue.StartsWith("{{BADPARSE}}:"))
                            {
                                string errorMessage = string.Format("Could not parse from position {0} for length of {1}. Check line length against File Profile.", rowDef.ParseStartPosition, rowDef.ParseLength);
                                throw new Exception(errorMessage);
                            }

                            usedFieldIndexes.Add(rowDef.SourceColumnNumber.Value - 1);

                            IColumn column = new Column(fieldValue, context);
                            if (rowDef.IsCustom > 0)
                            {
                                scriptRows[rowDef] = column;
                            }

                            row.Columns.Add(column);
                            row.OriginalRowLine = fields[fields.Length - 1];
                        }
                    }

                    if (!_fileToProcess.IsFixedWidth && !_fileToProcess.IsMultiSchema && _fileToProcess.CaptureUnmappedDataForInboundFile)
                    {
                        if ((!_fileToProcess.CaptureUnmappedDataOnlyWithHeaders) || (_fileToProcess.CaptureUnmappedDataOnlyWithHeaders && _headerFields != null))
                        {
                            _headerFields[_headerFields.Length - 1] = null;
                            for (var i = 0; i < fields.Length - 1; i++)
                            {
                                string field = fields[i];
                                if (!usedFieldIndexes.Contains(i))
                                {
                                    string headerField = (_headerFields == null || _headerFields.Length < i) ? null : _headerFields[i];
                                    if (_fileToProcess.CaptureUnmappedDataOnlyWithHeaders && headerField == null)
                                        continue;
                                    if (headerField == null)
                                        headerField = "UnmappedColumn" + i.ToString("000");
                                    IContext pbContext = new Context(DataTypeEnum.String, false, false,
                                                                   null, _fileToProcess.FileTypes[0].Description,
                                                                   null,
                                                                   null, 0,
                                                                   headerField, headerField,
                                                                   "PropertyBag",
                                                                   headerField);
                                    IColumn column = new Column(field, pbContext);
                                    row.Columns.Add(column);
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<RowDefinition, IColumn> scriptRow in scriptRows)
                    {
                        if (scriptRow.Key.IsCustom == 1)
                        {
                            rdsc = null;
                            string path = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
                            rdsc = new RowDefinitionScriptConfiguration();
                            rdsc.OverridePath = path;
                            rdsc.CompilerOptions.OutputFilename = Path.Combine(path, "AI." + Guid.NewGuid().ToString("D") + ".dll");
                            rdsc.SourceCode = rdsc.RequiredMethodSignatureBegin(true) + Environment.NewLine + scriptRow.Key.AIScriptSource + Environment.NewLine + rdsc.RequiredMethodSignatureEnd();
                            rdsc.ScriptEngine.Compile();
                            rdsc.RowDefinitionScriptCallMethod(row, scriptRow.Value);
                            MbroHelper.SetNullIdentity(row);
                            MbroHelper.SetNullIdentity(scriptRow.Value);
                            rdsc.ScriptEngine.Dispose();
                            rdsc.Dispose();
                            rdsc = null;
                        }
                        else if (scriptRow.Key.IsCustom == 2)
                        {
                            _scriptRunner.RunScript(scriptRow.Key.CustomAssemblyPath, scriptRow.Key.CustomClassName, scriptRow.Value, row, null);
                        }
                    }
                }
                catch (Exception e)
                {
                    errorType = HandleParsingWarnings(exceptions, errorType, rowDef, e);
                    row.RowError = errorType;
                    if (rdsc != null)
                    {
                        MbroHelper.SetNullIdentity(row);
                        rdsc.ScriptEngine.Dispose();
                        rdsc.Dispose();
                        rdsc = null;
                    }
                }
                _currentLineNumber++;
                yield return row;
            }
        }

        private string HandleParsingWarnings(List<Exception> exceptions, string errorType, RowDefinition rowDef, Exception e)
        {
            string columnDesc = string.Empty;
            if (rowDef != null)
                columnDesc = string.Format("Field '{0}', ", rowDef.FieldDisplayName);

            Exception parseEx = new Exception("Parsing Warning: " + string.Format("Unable to parse line {0}, {1} column {2}.  Error: {3}", _currentLineNumber + 1,
                                             columnDesc, (rowDef != null && rowDef.SourceColumnNumber.HasValue ? rowDef.SourceColumnNumber.Value : -1), e.Message));
            exceptions.Add(parseEx);

            if (_fileToProcess.MaximumParsingErrors > 0 && exceptions.Count >= _fileToProcess.MaximumParsingErrors)
            {
                errorType = string.Empty;
                throw new Exception(string.Format("Aborting import after {0} Parsing Warnings", _fileToProcess.MaximumParsingErrors));
            }

            return parseEx.Message;
        }

        private void ValidateProfileDefinition(object lookupValue)
        {
            string checkRowsErrors = string.Empty;
            if (!_fileToProcess.IsFixedWidth)
            {
                foreach (RowDefinition checkRowDef in _fileToProcess.FileTypes[0].RowDefinitions)
                {
                    if (checkRowDef.IsRequired &&
                        (!checkRowDef.SourceColumnNumber.HasValue || checkRowDef.SourceColumnNumber.Value <= 0))
                    {
                        checkRowsErrors += checkRowDef.FieldDisplayName + " is a required field but has no definition in the profile." +
                                           Environment.NewLine;
                    }
                }
            }


            if ((_fileToProcess.SchemaDetector != null) && (_fileToProcess.IsMultiSchema && _fileToProcess.SchemaDetector.IsRequired &&
                (!_fileToProcess.SchemaDetector.SourceColumnNumber.HasValue || _fileToProcess.SchemaDetector.SourceColumnNumber.Value <= 0)))
            {
                checkRowsErrors += _fileToProcess.SchemaDetector.FieldDisplayName + " is a required field but has no definition in the profile." +
                                   Environment.NewLine;
            }

            if (null != lookupValue)
            {
                foreach (RowDefinition checkRowDef in _fileToProcess.FileTypes.Single(ft => ft.SchemaIdentifier == lookupValue.ToString()).RowDefinitions)
                {
                    if (checkRowDef.IsRequired &&
                        (!checkRowDef.SourceColumnNumber.HasValue || checkRowDef.SourceColumnNumber.Value <= 0))
                    {
                        checkRowsErrors += checkRowDef.FieldDisplayName +
                                            " is a required field but has no definition in the profile." +
                                            Environment.NewLine;
                    }
                }
            }

            checkRowsErrors = checkRowsErrors.Trim();
            if (!string.IsNullOrWhiteSpace(checkRowsErrors))
            {
                throw new Exception(checkRowsErrors);
            }
        }

        private void SkipHeaderLines()
        {
            //skip header lines if defined
            if (_headerLines > 0)
            {
                int skipHeaderLines = 0;
                while (skipHeaderLines < _headerLines)
                {
                    _parser.ReadLine();
                    skipHeaderLines++;
                    _currentLineNumber++;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileStream.TryDispose();
                _parser.TryDispose();
            }
        }
    }
}