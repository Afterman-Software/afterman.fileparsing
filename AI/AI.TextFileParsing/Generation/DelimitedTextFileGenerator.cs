using System.IO;

namespace AI.TextFileParsing.Generation
{
    public class DelimitedTextFileGenerator : ITextFileGenerator
    {
        private readonly StreamWriter _writer;
        private long _lineCounter;

        private string _delimiter = ",";
        private string _textQualifier = "\"";

        public DelimitedTextFileGenerator(StreamWriter streamWriter)
        {
            _writer = streamWriter;
        }

        public long LineNumber
        {
            get { return _lineCounter; }
        }

        public string Delimiter
        {
            get
            {
                return _delimiter;
            }
            set
            {
                _delimiter = value;
            }
        }

        public string TextQualifier
        {
            get
            {
                return _textQualifier;
            }
            set
            {
                _textQualifier = value;
            }
        }

        public void WriteFields(string[] fields)
        {
            var fieldMax = fields.Length - 1;
            for (var i = 0; i <= fieldMax; i++)
            {
                string field = fields[i];
                var surround = field.Contains(_delimiter);
                var escape = field.Contains(_textQualifier);
                if (escape)
                {
                    surround = true;
                    field = field.Replace(_textQualifier, _textQualifier + _textQualifier);
                }
                if (surround)
                {
                    field = _textQualifier + field + _textQualifier;
                }
                if (i > 0 && i <= fieldMax)
                {
                    _writer.Write(_delimiter);
                }
                _writer.Write(field);
                if (i == fieldMax)
                {
                    _writer.WriteLine();
                }
            }
            _lineCounter++;
        }

        public void WriteLine()
        {
            _writer.WriteLine();
            _lineCounter++;
        }

        public void Close()
        {
            _writer.Close();
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
                    if (_writer != null)
                    {
                        _writer.Close();
                        _writer.Dispose();
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