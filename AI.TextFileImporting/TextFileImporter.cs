using System;
using System.Collections.Generic;
using AI.nRepo;
using AI.TextFileParsing;
using AI.TextFileParsing.DSL;
using AI.TextFileParsing.ParsingDefinitions;
using AI.TextFileParsing.Services;

namespace AI.TextFileImporting
{
	public class TextFileImporter<T> where T : class , new()
	{
		private readonly FileProfile _fileToImport;
		private readonly IRepository<T> _repository;
        private readonly IDslScriptRunner _scriptRunner;
        private readonly ICustomLogicContainer _customLogicContainer;

		public TextFileImporter(FileProfile fileToImport, IRepository<T> repository, IDslScriptRunner scriptRunner, ICustomLogicContainer customLogicContainer)
		{
			_fileToImport = fileToImport;
			_repository = repository;
            _scriptRunner = scriptRunner;
            _customLogicContainer = customLogicContainer;
		}

        //public FileImportStatusEnum ImportTextFile()
        //{
        //    List<Exception> exList = new List<Exception>();
        //    FileImportStatusEnum status = FileImportStatusEnum.Error;
        //    try
        //    {
        //        Dictionary<object, Table> tables;
        //        using (TextFileParser parser = new TextFileParser(_fileToImport, _scriptRunner, _customLogicContainer))
        //        {
        //            tables = new Dictionary<object, Table>();
        //            tables[0] = parser.Parse(exList);
        //        }

        //        foreach (var table in tables)
        //        {
        //            List<T> list = TableToTypeTranslator.GetTypedList<T>(table.Value);
        //            _repository.BeginTransaction();
        //            _repository.Add(list);
        //            _repository.CommitTransaction();
        //        }

        //        status = FileImportStatusEnum.ImportedSuccessfully;
        //    }
        //    catch (Exception e)
        //    {
        //        exList.Add(e);
        //        status = FileImportStatusEnum.Error;
        //    }

        //    if (status == FileImportStatusEnum.ImportedSuccessfully && exList.Count > 0)
        //    {
        //        status = FileImportStatusEnum.ImportedWithExceptions;
        //    }

        //    return status;
        //}
	}
}