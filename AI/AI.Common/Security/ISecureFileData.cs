using System.IO;
using System.Security.Cryptography;

namespace AI.Common.Security
{
    public interface ISecureFileData : IFileData
    {
        long Save(Stream stream, ICryptoTransform encryptor);
        Stream GetReadStream(ICryptoTransform decryptor);
        long WriteTo(Stream stream, ICryptoTransform decryptor);
        StreamReader GetStreamReader(ICryptoTransform decryptor);
        int GetLineCount(ICryptoTransform decryptor);
        StreamWriter GetStreamWriter(ICryptoTransform encryptor);
        Stream GetWriteStream(ICryptoTransform encryptor);
        StreamWriter GetAppendStreamWriter(ICryptoTransform encryptor);
    }
}