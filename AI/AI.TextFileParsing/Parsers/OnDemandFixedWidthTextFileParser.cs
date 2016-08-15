using System;

namespace AI.TextFileParsing.Parsers
{
    public class OnDemandFixedWidthTextFileParser : IOnDemandTextFileParser
    {
        public string[] CommentTokens { get; set; }

        public bool EndOfData
        {
            get { return false; }
        }

        public long LineNumber
        {
            get { return -1; }
        }

        public string ErrorLine
        {
            get { return null; }
        }

        public long ErrorLineNumber
        {
            get { return -1; }
        }

        public string[] ReadFields()
        {
            throw new NotImplementedException();
        }

        public string ReadLine()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            //no-op
            //closing should have no effect as this on-demand parser does nothing that needs to be closed
        }

        public void Dispose()
        {
            //no-op
            //disposing should have no effect as this on-demand parser does nothing that needs to be disposed
        }

        public string ReadField(int fieldIndex)
        {
            throw new NotImplementedException();
        }

        public string ReadField(string fieldName)
        {
            throw new NotImplementedException();
        }
    }
}