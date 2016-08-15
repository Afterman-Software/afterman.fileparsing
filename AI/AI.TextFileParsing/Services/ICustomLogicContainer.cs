using AI.Common.Security;
using AI.TextFileParsing.Interfaces;
using AI.TextFileParsing.ParsingDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.TextFileParsing.Services
{
    public interface ICustomLogicContainer : IDisposable
    {
        void Initialize(FileProfile fileProfile);

        IFileData PreParse(IFileData fileData);

        Dictionary<object, ITable> PostParse(Dictionary<object, ITable> table);
    }

    public class CustomLogicContainer : ICustomLogicContainer
    {

        private ICustomParseLogic _customLogicInstance;
        private AppDomain _customLogicAppDomain;
        private bool _initialized;

        public CustomLogicContainer()
        {

        }

        private void Guard()
        {
            if (!_initialized)
                throw new InvalidOperationException("Must Initialize the CustomLogicContainer before using it");
        }

        public IFileData PreParse(IFileData fileData)
        {
            Guard();
            if (_customLogicInstance != null)
            {
                try
                {
                   fileData = _customLogicInstance.PreParseFile(fileData);
                }
                catch (NotImplementedException niex)
                {
                    Debug.WriteLine(niex.ToString());
                }
            }

            return fileData;
        }

        public Dictionary<object, ITable> PostParse(Dictionary<object, ITable> tables)
        {
            Guard();
            if (_customLogicInstance != null)
            {
                try
                {
                    return _customLogicInstance.PostParseFile(tables);
                }
                catch (NotImplementedException niex)
                {
                    Debug.WriteLine(niex.ToString());
                }
            }
            return tables;
        }

        public void Initialize(FileProfile fileProfile)
        {
            _initialized = true;
            if (string.IsNullOrWhiteSpace(fileProfile.CustomLogicDllPath) || string.IsNullOrWhiteSpace(fileProfile.CustomLogicClassName))
            {
                return;
            }
            string[] customLogicAssemblyAndType = fileProfile.CustomLogicClassName.Split(new[] { ',' });
            if (customLogicAssemblyAndType.Length != 2)
            {
                return;
            }
            try
            {
                AppDomainSetup clads = new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                    PrivateBinPath = AppDomain.CurrentDomain.RelativeSearchPath,
                    ShadowCopyFiles = "true",
                    DisallowCodeDownload = true
                };
                //TODO: configuration file?
                _customLogicAppDomain = AppDomain.CreateDomain("Secondary Scripting Domain", null, clads);
                string customLogicDllFile = customLogicAssemblyAndType[0].Trim();
                if (!customLogicDllFile.ToLower().EndsWith(".dll"))
                    customLogicDllFile += ".dll";
                customLogicDllFile = Path.Combine(fileProfile.CustomLogicDllPath, customLogicDllFile);
                _customLogicInstance = (ICustomParseLogic)_customLogicAppDomain.CreateInstanceFromAndUnwrap(customLogicDllFile, customLogicAssemblyAndType[1].Trim());
            }
            catch (Exception e)
            {
                _customLogicInstance = null;
                throw new TypeLoadException("The custom parse logic type could not be loaded. [" + customLogicAssemblyAndType + "]", e);
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
                    if (_customLogicAppDomain != null)
                        AppDomain.Unload(_customLogicAppDomain);
                }
                catch
                {
                    //swallow exceptions and just dispose
                }
            }
        }


    }

}
