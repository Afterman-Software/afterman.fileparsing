using System.Collections.Generic;

namespace AI.TextFileParsing.Interfaces
{
    public interface IRow
    {
        IList<IColumn> Columns
        {
            get;
        }
        int OriginalLineNumber { get; set; }
        string OriginalRowLine { get; set; }

        string RowError { get; set; }
    }
}