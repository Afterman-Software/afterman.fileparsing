using System;
using System.Collections.Generic;
using AI.Common.Security;

namespace AI.TextFileParsing
{
    public class FileParsingResult
    {
        public FileParsingResult()
        {
            TemporaryFiles = new Dictionary<object, IFileData>();
            Tables = new Dictionary<object, Table>();
            Errors = new List<Exception>();
            EdiClaimXml = new List<string>();
        }

        public long TotalLines { get; set; }
        public long HeaderLines { get; set; }
        public long FooterLines { get; set; }
        public long BlankLines { get; set; }
        public long ErrorLines { get; set; }
        public long DataLines { get; set; }
        public Dictionary<object, IFileData> TemporaryFiles { get; private set; }
        public Dictionary<object, Table> Tables { get; set; }
        public List<Exception> Errors { get; set; }
        public List<string> EdiClaimXml { get; set; }
    }
}