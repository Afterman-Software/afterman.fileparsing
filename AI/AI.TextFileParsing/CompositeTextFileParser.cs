using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AI.Common.Security;
using AI.TextFileParsing.DSL;
using AI.TextFileParsing.Enums;
using AI.TextFileParsing.Interfaces;
using AI.TextFileParsing.Parsers;
using AI.TextFileParsing.ParsingDefinitions;
using AI.TextFileParsing.Services;

namespace AI.TextFileParsing
{
    using AI.Common.Extensions.Sys;

    public class CompositeTextFileParser : IDisposable
    {
        private readonly FileParsingResult _result;
        private readonly FileProfile _fileToProcess;
        private readonly IDslScriptRunner _scriptRunner;
        private readonly IFileToStreamConverter _fileToStreamConverter;
        private readonly Dictionary<object, StreamWriter> _writers;
        private readonly ICustomLogicContainer _customLogicContainer;
        private int _currentLineNumber = 0;
        private readonly IFileData _fileData;
        private string[] _headerFields = null;
        private readonly bool _isEdiFileFormat;
        private int _startAtLineNumber = -1;

        public CompositeTextFileParser(FileProfile fileToProcess, IFileData fileData, IDslScriptRunner scriptRunner, ICustomLogicContainer customLogicContainer, bool isEdiFileFormat = false, int startAtLineNumber = -1)
        {
            _result = new FileParsingResult();
            _fileToProcess = fileToProcess;
            _scriptRunner = scriptRunner;
            _fileToStreamConverter = new FileToStreamConverter();
            _writers = new Dictionary<object, StreamWriter>();
            _customLogicContainer = customLogicContainer;
            _fileData = fileData;
            _isEdiFileFormat = isEdiFileFormat;
            _startAtLineNumber = startAtLineNumber;
            SplitSchemas();
        }

        private void SplitSchemas()
        {
            if (!_isEdiFileFormat)
            {
                long totalLines = _fileData.GetLineCount();
                _result.TotalLines = totalLines;

                if (!_fileToProcess.IsFixedWidth && (_fileToProcess.HeaderLines.HasValue && _fileToProcess.HeaderLines > 0) && _fileToProcess.CaptureUnmappedDataForInboundFile)
                {
                    Stream headerStream = null;
                    DelimitedTextFileParser headerParser = null;
                    try
                    {
                        headerStream = _fileData.GetReadStream();
                        headerParser = new DelimitedTextFileParser(headerStream);
                        headerParser.SetDelimiters(_fileToProcess.Delimiter);
                        headerParser.SetTextQualifier(_fileToProcess.TextQualifier);
                        _headerFields = headerParser.ReadFields();

                        //BK : This code is doing nothing and spending cycle just finding byteread which is never used. 
                        //this takes around 15 minutes with big file.
                        //int byteRead = 0;
                        //while (byteRead != -1)
                        //{
                        //    byteRead = headerStream.ReadByte();
                        //}
                    }
                    catch
                    {
                        //swallow, ignore headers
                        _headerFields = null;
                    }
                    finally
                    {
                        if (headerStream != null)
                        {
                            try
                            {
                                headerStream.Dispose();
                            }
                            catch
                            {
                                //swallow errors
                                //hopefully the stream eventually gets disposed by GC
                            }
                        }
                        if (headerParser != null)
                            headerParser.Dispose();
                    }
                }
                if (!_fileToProcess.IsMultiSchema)
                {
                    _result.TemporaryFiles.Add("Default", _fileData);
                    return;
                }

                long footerLines = _fileToProcess.FooterLines.HasValue ? _fileToProcess.FooterLines.Value : 0;
                StreamReader readReader = _fileData.GetStreamReader();
                StreamReader parserReader = _fileData.GetStreamReader();

                ITextFileParser schemaParser = ParserFactory.Instance.GetSchemaParser<ITextFileParser>(_fileToProcess, parserReader.BaseStream);

                SkipHeaderLines(readReader, schemaParser);

                while (!schemaParser.EndOfData)
                {
                    _currentLineNumber++;
                    if (footerLines > 0 && _currentLineNumber > totalLines - footerLines)
                    {
                        break;
                    }

                    string[] fields = schemaParser.ReadFields();
                    if (fields == null)
                    {
                        _result.BlankLines++;
                        readReader.ReadLine();
                        continue;
                    }

                    RowDefinition rowDef = _fileToProcess.SchemaDetector;
                    IContext context = new Context((DataTypeEnum)rowDef.DataType, rowDef.IsRequired, rowDef.CanSetDefault,
                                                   rowDef.DefaultValue, _fileToProcess.FileTypes[0].Description,
                                                   (_fileToProcess.IsFixedWidth ? null : rowDef.ParseStartPosition),
                                                   (_fileToProcess.IsFixedWidth ? null : rowDef.ParseLength), rowDef.FieldFormat,
                                                   rowDef.FieldDisplayName, rowDef.FieldDescription, rowDef.TargetTableName,
                                                   rowDef.TargetFieldName);

                    IColumn column = (_fileToProcess.IsFixedWidth ? new Column(fields[0], context) : new Column(fields[rowDef.SourceColumnNumber.Value - 1], context));

                    //TODO: CryptoStream seems to convert blank lines into a bunch of escaped nulls
                    //is there a better way to detect this condition, or prevent it all together
                    if (column.ActualValue != null && column.ActualValue.ToString() != "\0\0")
                    {
                        var currentWriter = GetStreamWriter(column.ActualValue);
                        string line = readReader.ReadLine();
                        int originalLineLength = line.Length;
                        const int rowNumLength = 10;
                        if (_fileToProcess.IsFixedWidth)
                        {
                            line += _currentLineNumber.ToString().ToFixedLength(rowNumLength, ' ', true);
                        }
                        else
                        {
                            line += (_fileToProcess.Delimiter.ToLower().Trim() == "tab" ? "\t" : _fileToProcess.Delimiter) + _currentLineNumber.ToString();
                        }
                        currentWriter.WriteLine(line);
                    }
                }

                CloseWriterStreams();
            }
            else
            {
                _result.TemporaryFiles.Add("EdiDefault", _fileData);
            }
        }

        public FileParsingResult Parse()
        {
            if (!_isEdiFileFormat)
            {
                _result.Tables = new Dictionary<object, Table>();
                _result.Errors = new List<Exception>();
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    foreach (var x in _result.TemporaryFiles)
                    {
                        ProcessSingleFile(x.Key, x.Value, _result, _startAtLineNumber);
                    }
                }
                else
                {
                    System.Threading.Tasks.Parallel.ForEach(_result.TemporaryFiles, x =>
                    {
                        ProcessSingleFile(x.Key, x.Value, _result, _startAtLineNumber);
                    });
                }

                return _result;
            }
            else
            {
                _result.EdiClaimXml = new List<string>();
                _result.Errors = new List<Exception>();
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    foreach (var x in _result.TemporaryFiles)
                    {
                        ProcessEdiFile(x.Value, _result);
                    }
                }
                else
                {
                    System.Threading.Tasks.Parallel.ForEach(_result.TemporaryFiles, x =>
                    {
                        ProcessEdiFile(x.Value, _result);
                    });
                }

                return _result;
            }
        }

        private void ProcessSingleFile(object schema, IFileData fileData, FileParsingResult result, int startAtLineNumber)
        {
            var fileProfile = this._fileToProcess.GetDeepClone();
            fileProfile.FileProfileId = -1;
            fileProfile.FileName = fileData.OriginalFileName;
            fileProfile.IsMultiSchema = false;
            fileProfile.HeaderLines = 0;
            if (!_fileToProcess.IsMultiSchema)
            {
                fileProfile.HeaderLines = _fileToProcess.HeaderLines;
            }
            fileProfile.FooterLines = 0;
            var correctFileTypes = from x in _fileToProcess.FileTypes
                                   where x.SchemaIdentifier == schema.ToString()
                                   select x;
            fileProfile.FileTypes = correctFileTypes.ToList();
            if (fileProfile.FileTypes != null && fileProfile.FileTypes.Count == 1)
            {
                if (_fileToProcess.IsFixedWidth)
                {
                    int scn = 0;
                    foreach (var rd in fileProfile.FileTypes[0].RowDefinitions)
                    {
                        scn++;
                        rd.SourceColumnNumber = scn;
                    }
                }
                var parser = new TextFileParser(fileProfile, fileData, _scriptRunner, _headerFields, startAtLineNumber);
                var exceptions = new List<Exception>();
                var table = parser.Parse(exceptions);
                lock (this)
                {
                    result.Errors.AddRange(exceptions);
                    result.Tables[schema] = table;
                }
            }
        }

        private void ProcessEdiFile(IFileData fileData, FileParsingResult result)
        {
            var parser = new EdiTextFileParser(fileData);
            var exceptions = new List<Exception>();
            var ediClaimXml = parser.Parse(exceptions);
            lock (this)
            {
                result.Errors.AddRange(exceptions);
                result.EdiClaimXml = ediClaimXml;
            }
        }

        private StreamWriter GetStreamWriter(object schemaId)
        {
            if (_writers.ContainsKey(schemaId))
            {
                return _writers[schemaId];
            }

            var fileData = new SecureFileData(SharedTempFile.NewFile());
            _result.TemporaryFiles.Add(schemaId, fileData);
            var streamWriter = fileData.GetStreamWriter();
            _writers.Add(schemaId, streamWriter);

            return streamWriter;
        }

        private void SkipHeaderLines(StreamReader readReader, ITextFileParser parser)
        {
            int? headerLines = _fileToProcess.HeaderLines;
            if (headerLines.HasValue && headerLines.Value > 0)
            {
                int skipHeaderLines = 0;
                while (skipHeaderLines < headerLines.Value)
                {
                    readReader.ReadLine();
                    parser.ReadLine();
                    skipHeaderLines++;
                    _result.HeaderLines++;
                    _currentLineNumber++;
                }
            }
        }

        private void CloseWriterStreams()
        {
            try
            {
                foreach (var kvp in _writers)
                {
                    if (kvp.Value != null)
                        kvp.Value.Close();
                }
            }
            catch
            {
                //swallow exceptions and just dispose
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
                try
                {
                    foreach (var kvp in _writers)
                    {
                        if (kvp.Value != null)
                            kvp.Value.Dispose();
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