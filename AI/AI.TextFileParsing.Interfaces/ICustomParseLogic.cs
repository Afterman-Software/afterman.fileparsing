using System.Collections.Generic;
using System.IO;
using AI.Common.Security; // TODO: Is this ok?


namespace AI.TextFileParsing.Interfaces
{
	public interface ICustomParseLogic
	{
        IFileData PreParseFile(IFileData input);

        Dictionary<object, ITable> PostParseFile(Dictionary<object, ITable> tables);
	}
}