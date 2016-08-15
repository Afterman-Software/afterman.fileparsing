namespace AI.TextFileImporting
{
	public enum FileImportStatusEnum : int
	{
		Pending = 0,
		Importing = 1,
		ImportedSuccessfully = 2,
		ImportedWithExceptions = 3,
		Error = 4
	}
}