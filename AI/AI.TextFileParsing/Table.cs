using System;
using System.Collections.Generic;
using System.Linq;
using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing
{
     [Serializable]
	public class Table : MarshalByRefObject, ITable
	{
		
		public Table()
		{
			
		}

        public IEnumerable<IRow> Rows
        {
            get;
            set;
        }

		public IList<IRow> GetRowsByIndexes(params int[] indexes)
		{
			IList<IRow> rowList = new List<IRow>();
			if (indexes != null && indexes.Length > 0)
			{
				foreach (int index in indexes)
				{
                    var currentIndex = 0;
                    foreach (var row in Rows)
                    {
                        if (currentIndex == index)
                        {
                            rowList.Add(row);
                        }
                        currentIndex++;
                    }
				}
			}
			return rowList;
		}

		public IList<IColumn> GetColumnsByIndex(int index)
		{
			return Rows.Select(row => row.Columns[index]).ToList();
		}

		internal void ApplyContext()
		{
			foreach (Row row in Rows)
			{
				row.ApplyContext();
			}
		}

		internal bool ValidateContext()
		{
			return Rows.Cast<Row>().Aggregate(true, (validated, row) => validated & row.ValidateContext());
		}
	}
}