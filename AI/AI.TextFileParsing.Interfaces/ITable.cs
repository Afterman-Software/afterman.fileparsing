using System.Collections.Generic;

namespace AI.TextFileParsing.Interfaces
{
	public interface ITable
	{
        IEnumerable<IRow> Rows
        {
            get;
            set;
        }
		IList<IRow> GetRowsByIndexes(params int[] indexes);
		IList<IColumn> GetColumnsByIndex(int index);
	}
}