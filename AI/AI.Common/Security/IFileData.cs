using System.Collections.Generic;
using System.IO;

namespace AI.Common.Security
{
    public interface IFileData
    {
        string OriginalFileName { get; set; }
        string BackingFilePath { get; set; }
        long OriginalFileLength { get; }
        bool HasData { get; }
        long Save(Stream stream);
        Stream GetReadStream();
        long WriteTo(Stream stream);
        StreamReader GetStreamReader();
        int GetLineCount();
        StreamWriter GetStreamWriter();
        Stream GetWriteStream();
        StreamWriter GetAppendStreamWriter();
        void Truncate();
        void AppendFileData(IEnumerable<IFileData> appendData, string mergeMode = "0");
        int CurrentLineCount { get; }
    }
}