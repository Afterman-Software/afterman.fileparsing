using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AI.Common.Security
{
    [Serializable]
    public class SecureFileData : ISecureFileData
    {
        private const int _bufferSize = 4096;

        public string OriginalFileName { get; set; }

        private string _backingFilePath;
        public string BackingFilePath
        {
            get { return _backingFilePath; }
            set { _backingFilePath = value; }
        }

        private long _originalFileLength;
        public long OriginalFileLength
        {
            get { return _originalFileLength; }
        }

        public int CurrentLineCount { get; private set; }

        public SecureFileData(string backingFilePath, bool alsoTruncate = false)
        {
            _backingFilePath = backingFilePath;
            _originalFileLength = 0;

            if (alsoTruncate)
                Truncate();
        }

        public long Save(Stream stream, ICryptoTransform encryptor)
        {
            var bytesWritten = 0;

            using (var fileStream = new FileStream(_backingFilePath, FileMode.OpenOrCreate))
            using (var cryptoStream = new CryptoStream(fileStream, EncryptDecrypt.CreateEncryptor(), CryptoStreamMode.Write))
            {
                var buffer = new byte[_bufferSize];

                if (stream.CanRead)
                {
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);

                    while (bytesRead > 0)
                    {
                        cryptoStream.Write(buffer, 0, bytesRead);
                        bytesWritten += bytesRead;
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                    }
                }

                cryptoStream.Flush();
                cryptoStream.Close();
            }


            _originalFileLength = bytesWritten;

            return bytesWritten;
        }

        public Stream GetReadStream(ICryptoTransform decryptor)
        {
            if (string.IsNullOrEmpty(_backingFilePath) || !File.Exists(_backingFilePath))
                return new MemoryStream();

            if (File.Exists(_backingFilePath) && new FileInfo(_backingFilePath).Length == 0)
                return new MemoryStream();

            var fileStream = new FileStream(_backingFilePath, FileMode.Open, FileAccess.Read);
            var cryptoStream = new CryptoStream(fileStream, EncryptDecrypt.CreateDecryptor(), CryptoStreamMode.Read);

            return cryptoStream;
        }

        public bool HasData
        {
            get
            {
                return (File.Exists(_backingFilePath) && new FileInfo(_backingFilePath).Length > 0);
            }
        }

        public long Save(Stream stream)
        {
            return Save(stream, EncryptDecrypt.CreateEncryptor());
        }

        public Stream GetReadStream()
        {
            return GetReadStream(EncryptDecrypt.CreateDecryptor());
        }

        public long WriteTo(Stream stream)
        {
            return WriteTo(stream, EncryptDecrypt.CreateDecryptor());
        }

        public long WriteTo(Stream stream, ICryptoTransform decryptor)
        {
            if (!HasData)
                return 0;

            long bytesWritten = 0;

            using (var fileStream = new FileStream(_backingFilePath, FileMode.Open, FileAccess.Read))
            using (var cryptoStream = new CryptoStream(fileStream, EncryptDecrypt.CreateDecryptor(), CryptoStreamMode.Read))
            {
                var buffer = new byte[_bufferSize];
                var bytesRead = cryptoStream.Read(buffer, 0, buffer.Length);

                while (bytesRead > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                    bytesWritten += bytesRead;
                    bytesRead = cryptoStream.Read(buffer, 0, buffer.Length);
                }

                cryptoStream.Flush();
                cryptoStream.Close();
            }


            return bytesWritten;
        }

        public StreamReader GetStreamReader()
        {
            return GetStreamReader(EncryptDecrypt.CreateDecryptor());
        }

        public StreamReader GetStreamReader(ICryptoTransform decryptor)
        {
            return new StreamReader(GetReadStream(decryptor));
        }

        public int GetLineCount()
        {
            if (!HasData)
                return 0;

            return GetLineCount(EncryptDecrypt.CreateDecryptor());
        }

        public int GetLineCount(ICryptoTransform decryptor)
        {
            if (!HasData)
                return 0;
            if (LineCount > 0)
                return LineCount;
            var lineCount = 0;

            using (var readStream = GetReadStream(decryptor))
            using (var streamReader = new StreamReader(readStream))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                        lineCount++;
                }
            }
            LineCount = lineCount;
            return lineCount;
        }

        public int LineCount { get; set; }

        public StreamWriter GetStreamWriter()
        {
            return GetStreamWriter(EncryptDecrypt.CreateEncryptor());
        }

        public Stream GetWriteStream()
        {
            return GetWriteStream(EncryptDecrypt.CreateEncryptor());
        }

        public StreamWriter GetStreamWriter(ICryptoTransform encryptor)
        {
            return new StreamWriter(GetWriteStream(encryptor));
        }

        public Stream GetWriteStream(ICryptoTransform encryptor)
        {
            var fileStream = new FileStream(_backingFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);

            return cryptoStream;
        }

        public void Truncate()
        {
            try
            {
                if (File.Exists(_backingFilePath))
                {
                    using (var fs = new FileStream(_backingFilePath, FileMode.Truncate, FileAccess.Write))
                    {
                    }
                }
                _originalFileLength = 0;
                this.CurrentLineCount = 0;
            }
            catch (Exception ex)
            {
                //swallow error as the file just must not exist
                //this is for when it may have been partially created and just needs to clear before starting again
            }
        }

        public StreamWriter GetAppendStreamWriter()
        {
            return GetAppendStreamWriter(EncryptDecrypt.CreateEncryptor());
        }

        public StreamWriter GetAppendStreamWriter(ICryptoTransform encryptor)
        {
            var fileStream = new FileStream(_backingFilePath, FileMode.Append, FileAccess.Write);
            var cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write);

            return new StreamWriter(cryptoStream);
        }

        public void AppendFileData(IEnumerable<IFileData> appendData, string mergeMode = "0")
        {
            switch (mergeMode)
            {
                case "2":
                    AppendFileData2(appendData);
                    break;
                case "3":
                    AppendFileData3(appendData);
                    break;
                default:
                    AppendFileData1(appendData);
                    break;
            }
        }

        private void AppendFileData1(IEnumerable<IFileData> appendData)
        {
            try
            {
                using (var sw = GetAppendStreamWriter())
                {
                    foreach (var fileData in appendData)
                    {
                        using (var sr = fileData.GetStreamReader())
                        {
                            while (!sr.EndOfStream)
                            {
                                var line = sr.ReadLine();

                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    sw.WriteLine(line);
                                    this.CurrentLineCount++;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var log = "Application";
                    var source = "Secure File Data object";
                    if (!EventLog.SourceExists(source))
                        EventLog.CreateEventSource(source, log);
                    EventLog.WriteEntry(source, "SFD Error1: " + ex.ToString(), EventLogEntryType.Error);

                    throw;
                }
                catch (Exception ex2)
                {
                    //well, who knows
                    throw;
                }
            }
        }

        public void AppendFileData2(IEnumerable<IFileData> appendData)
        {
            try
            {

                using (var sw = GetAppendStreamWriter())
                {
                    foreach (var fileData in appendData)
                    {
                        using (var sr = fileData.GetStreamReader())
                        {
                            while (!sr.EndOfStream)
                            {
                                var line = sr.ReadLine();

                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    sw.WriteLine(line);
                                    this.CurrentLineCount++;
                                }
                            }
                        }

                        try
                        {
                            File.Delete(fileData.BackingFilePath);
                            //File.Move(fileData.BackingFilePath, fileData.BackingFilePath + "ignoremeplease");
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                var log = "Application";
                                var source = "Secure File Data object";
                                if (!EventLog.SourceExists(source))
                                    EventLog.CreateEventSource(source, log);
                                EventLog.WriteEntry(source, "SFD Error2 Delete: " + ex.ToString(), EventLogEntryType.Error);

                                throw;
                            }
                            catch (Exception ex2)
                            {
                                //well, who knows
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var log = "Application";
                    var source = "Secure File Data object";
                    if (!EventLog.SourceExists(source))
                        EventLog.CreateEventSource(source, log);
                    EventLog.WriteEntry(source, "SFD Error2: " + ex.ToString(), EventLogEntryType.Error);

                    throw;
                }
                catch (Exception ex2)
                {
                    //well, who knows
                    throw;
                }
            }
        }

        private void AppendFileData3(IEnumerable<IFileData> appendData)
        {
            try
            {
                string batchSizeString = ConfigurationManager.AppSettings["AppenderBatchSize"];
                int batchSize;
                if (!int.TryParse(batchSizeString, out batchSize))
                {
                    batchSize = 60;
                }
                List<IFileData> appendingData = appendData.ToList();
                List<List<IFileData>> sendingData = new List<List<IFileData>>();
                List<IFileData> batchData = null;
                for (var i = 0; i < appendingData.Count; i++)
                {
                    if (batchData == null)
                    {
                        batchData = new List<IFileData>();
                    }

                    batchData.Add(appendingData[i]);

                    if ((batchData.Count == batchSize) || (i == appendingData.Count - 1))
                    {
                        sendingData.Add(batchData);
                        batchData = null;
                    }
                }

                foreach (var data in sendingData)
                {
                    AppendFileData3_1(data);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var log = "Application";
                    var source = "Secure File Data object";
                    if (!EventLog.SourceExists(source))
                        EventLog.CreateEventSource(source, log);
                    EventLog.WriteEntry(source, "SFD Error2: " + ex.ToString(), EventLogEntryType.Error);

                    throw;
                }
                catch (Exception ex2)
                {
                    //well, who knows
                    throw;
                }
            }
        }

        public void AppendFileData3_1(List<IFileData> appendData)
        {
            try
            {
                using (var sw = GetAppendStreamWriter())
                {
                    foreach (var fileData in appendData)
                    {
                        using (var sr = fileData.GetStreamReader())
                        {
                            while (!sr.EndOfStream)
                            {
                                var line = sr.ReadLine();

                                if (!string.IsNullOrWhiteSpace(line))
                                {
                                    sw.WriteLine(line);
                                    this.CurrentLineCount++;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var log = "Application";
                    var source = "Secure File Data object";
                    if (!EventLog.SourceExists(source))
                        EventLog.CreateEventSource(source, log);
                    EventLog.WriteEntry(source, "SFD Error3: " + ex.ToString(), EventLogEntryType.Error);

                    throw;
                }
                catch (Exception ex2)
                {
                    //well, who knows
                    throw;
                }
            }
        }
    }
}