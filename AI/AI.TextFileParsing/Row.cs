using System;
using System.Collections.Generic;
using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing
{
    [Serializable]
    public class Row : MarshalByRefObject, IRow
    {
        private readonly IList<IColumn> _columns;

        public Row()
        {
            _columns = new List<IColumn>();
        }

        public IList<IColumn> Columns
        {
            get
            {
                return _columns;
            }
        }

        public int OriginalLineNumber
        {
            get;
            set;
        }

        public string OriginalRowLine { get; set; }

        public string RowError { get; set; }

        internal void ApplyContext()
        {
            foreach (Column column in Columns)
            {
                object workingValue = column.ApplyContext();
                column.ActualValue = workingValue;
            }
        }

        internal bool ValidateContext()
        {
            bool validated = true;
            foreach (Column column in Columns)
            {
                validated = validated & column.ValidateContext(column.ActualValue);
            }
            return validated;
        }
    }
}