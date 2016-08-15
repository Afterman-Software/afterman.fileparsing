
using System.IO;
namespace AI.TextFileParsing.Services
{
    using AI.Common.Extensions.Sys;

    interface IFileToStreamConverter
	{
		Stream GetStream(byte[] bytes);

		Stream GetStream(string fileName);

		StreamReader GetStreamReader(byte[] bytes);

		StreamReader GetStreamReader(string fileName);

		Stream GetOutputStream(string fileName);

		StreamWriter GetOutputStreamWriter(string fileName);
	}

	public class FileToStreamConverter : IFileToStreamConverter
	{
		public Stream GetStream(byte[] bytes)
		{
		    var temp = SharedTempFile.NewFile();
			using (var fileStream = File.Open(temp, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				fileStream.Write(bytes, 0, bytes.Length);
			}
			return File.Open(temp, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public Stream GetStream(string fileName)
		{
			return File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		}

		public StreamReader GetStreamReader(byte[] bytes)
		{
			Stream stream = GetStream(bytes);
			return new StreamReader(stream);
		}

		public StreamReader GetStreamReader(string fileName)
		{
			Stream stream = GetStream(fileName);
			return new StreamReader(stream);
		}

		public Stream GetOutputStream(string fileName)
		{
			return File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
		}

		public StreamWriter GetOutputStreamWriter(string fileName)
		{
			Stream stream = GetOutputStream(fileName);
			return new StreamWriter(stream);
		}
	}
}