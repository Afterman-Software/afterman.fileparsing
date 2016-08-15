using System;

namespace AI.Common.Extensions.Sys
{
    using System.Configuration;
    using System.IO;

    public class SharedTempFile
    {
        public static string NewFile(ITempFileService tempFileService = null)
        {
            if (tempFileService == null)
                tempFileService = new TempFileService();
            
            return tempFileService.NewFile();
        }

        public static string NewFileName(ITempFileService tempFileService = null)
        {
            if (tempFileService == null)
                tempFileService = new TempFileService();

            return tempFileService.NewFileName();
        }

        public static string NewShortNameFile(ITempFileService tempFileService = null)
        {
            if (tempFileService == null)
                tempFileService = new TempFileService();

            return tempFileService.NewShortNameFile();
        }
    }

    public interface ITempFileService
    {
        string NewFile();
        string NewShortNameFile();
        void RemoveDirectory(string directoryPath);
        DirectoryInfo GetDirectory(string directoryPath);
        string GetDataBusDirectory();
        string NewFileName();
    }

    public class TempFileService : ITempFileService
    {
        public TempFileService()
        {
            this.TempFolder = ConfigurationManager.AppSettings["TempFileDrop"];
            EnsureRootDirExists();
        }

        protected string TempFolder { get; private set; }

        public string NewFile()
        {
            var fullFileName = NewFileName();
            using (File.Create(fullFileName))
            {
            }
            return fullFileName;

        }

        public string NewFileName()
        {
            var newFileName = Guid.NewGuid().ToString().Replace("-", String.Empty) + ".temp";
            var fullFileName = System.IO.Path.Combine(TempFolder, newFileName);

            return fullFileName;
        }

        public string NewShortNameFile()
        {
            var tmp = System.IO.Path.GetTempFileName();
            var file = new FileInfo(tmp).Name;
            var newFileName = file;
            var fullFileName = System.IO.Path.Combine(TempFolder, newFileName);
            using (File.Create(fullFileName))
            {
            }
            return fullFileName;
        }

        private void EnsureRootDirExists()
        {
            if (!Directory.Exists(TempFolder))
                Directory.CreateDirectory(TempFolder);
        }

        public void RemoveDirectory(string directoryPath)
        {
            try
            {
                RemoveDirectoryInternal(directoryPath);
            }
            catch(Exception)
            {
            }
        }

        private void RemoveDirectoryInternal(string directoryPath)
        {
            var dirPath = Path.Combine(TempFolder, directoryPath);
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
        }

        public DirectoryInfo GetDirectory(string directoryPath)
        {
            var dirPath = Path.Combine(TempFolder, directoryPath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(dirPath);

            return new DirectoryInfo(dirPath);
        }

        public string GetDataBusDirectory()
        {
            var dataBusPath = ConfigurationManager.AppSettings["ESB.FileShareDataBus"];

            return dataBusPath;
        }
    }
}