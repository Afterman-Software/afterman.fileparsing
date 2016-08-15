using System;
using System.Globalization;
using System.Linq;
using AI.TextFileParsing.Enums;
using AI.TextFileParsing.Interfaces;
using AI.TextFileParsing.ParseException;

namespace AI.TextFileParsing
{
    [Serializable]
    public class Column : MarshalByRefObject, IColumn
    {
        private bool _partialContextApplied;

        private readonly string _originalValue;
        private string _stringValue;
        private readonly IContext _context;
        private object _actualValue;

        public Column
            (
                string originalValue,
                IContext context
            )
        {
            _originalValue = originalValue;
            _context = context;
            object workingValue = ApplyContext();
            if (ValidateContext(workingValue))
                _actualValue = workingValue;
        }

        public string OriginalValue
        {
            get
            {
                return _originalValue;
            }
        }

        public string StringValue
        {
            get
            {
                return _stringValue;
            }
            set
            {
                _stringValue = value;
                object workingValue = ApplyContext();
                if (ValidateContext(workingValue))
                    _actualValue = workingValue;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", this.Context.FieldDisplayName, this.ActualValue);
        }

        public IContext Context
        {
            get
            {
                return _context;
            }
        }

        public object ActualValue
        {
            get
            {
                return _actualValue;
            }
            set
            {
                if (ValidateContext(value))
                    _actualValue = value;
            }
        }

        internal object ApplyContext()
        {
            Context workingContext = (Context)_context;
            string workingStringValue = null;
            object workingValue;

            try
            {
                if (!_partialContextApplied)
                {
                    workingStringValue = _originalValue;
                    if (string.IsNullOrWhiteSpace(workingStringValue) && workingContext.HasDefaultValue &&
                        !string.IsNullOrWhiteSpace(workingContext.DefaultValue))
                    {
                        workingStringValue = workingContext.DefaultValue;
                    }
                    else if (!string.IsNullOrWhiteSpace(workingStringValue) && workingContext.SubstringStart.HasValue &&
                             workingContext.SubstringStart.Value >= 0 && workingContext.SubstringLength.HasValue &&
                             workingContext.SubstringLength.Value > 0)
                    {
                        workingStringValue = workingStringValue.Substring(workingContext.SubstringStart.Value - 1,
                            workingContext.SubstringLength.Value);
                    }
                }
                else
                {
                    workingStringValue = _stringValue;
                }
                _partialContextApplied = true;

                _stringValue = workingStringValue;
                if (workingContext.DataTypeId == DataTypeEnum.Integer)
                {
                    if (string.IsNullOrWhiteSpace(workingStringValue))
                    {
                        workingValue = null;
                    }
                    else
                    {
                        if (workingStringValue.Trim() == "-")
                            workingStringValue = "0";

                        workingStringValue = ConvertFromEbcdicEncoding(workingStringValue);
                        workingValue = int.Parse(workingStringValue);
                    }
                }
                else if (workingContext.DataTypeId == DataTypeEnum.Date)
                {
                    workingValue = AI.TextFileParsing.Context.ParseDate(workingStringValue, workingContext.FieldFormatId);
                }
                else if (workingContext.DataTypeId == DataTypeEnum.Boolean)
                {
                    workingValue = AI.TextFileParsing.Context.ParseBoolean(workingStringValue);
                }
                else if (workingContext.DataTypeId == DataTypeEnum.Time)
                {
                    workingValue = AI.TextFileParsing.Context.ParseTime(workingStringValue, workingContext.FieldFormatId);
                }
                else if (workingContext.DataTypeId == DataTypeEnum.DecimalCurrency ||
                         workingContext.DataTypeId == DataTypeEnum.DecimalPercent)
                {
                    if (string.IsNullOrWhiteSpace(workingStringValue))
                    {
                        workingValue = null;
                    }
                    else
                    {
                        if (workingContext.DataTypeId == DataTypeEnum.DecimalPercent)
                            workingStringValue = workingStringValue.Replace("%", "").Replace(",", "");
                        else if (workingContext.DataTypeId == DataTypeEnum.DecimalCurrency)
                            workingStringValue = workingStringValue.Replace("$", "").Replace(",", "");
                        if (workingStringValue.Trim() == "-")
                            workingStringValue = "0.0";

                        workingStringValue = ConvertFromEbcdicEncoding(workingStringValue);

                        workingValue = decimal.Parse(workingStringValue, NumberStyles.Any);
                        if (workingContext.DataTypeId == DataTypeEnum.DecimalPercent)
                        {
                            workingValue = AI.TextFileParsing.Context.DivideByImpliedDecimalRate((decimal)workingValue,
                                workingContext.FieldFormatId);
                        }
                        else if (workingContext.DataTypeId == DataTypeEnum.DecimalCurrency)
                        {
                            workingValue = AI.TextFileParsing.Context.DivideByImpliedDecimalCurrency((decimal)workingValue,
                                workingContext.FieldFormatId);
                        }
                    }
                }
                else //workingContext.DataTypeId == 2 //varchar,string
                {
                    workingValue = workingStringValue.Trim();
                }
            }
            catch (Exception e)
            {
                throw new ParseConversionException(_context, workingStringValue, e);    
            }

            return workingValue;
        }

        public static string ConvertFromEbcdicEncoding(string workingStringValue)
        {
            if (workingStringValue.Length > 0)
            {
                char[] positiveChars = {'{', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I'};
                char[] negativeChars = {'}', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R'};
                char lastCharFromWorkingStringValue = workingStringValue[workingStringValue.Length - 1];

                int index = -1;
                index = Array.IndexOf(positiveChars, lastCharFromWorkingStringValue);
                if (index > -1)
                {
                    workingStringValue = workingStringValue.Substring(0, workingStringValue.Length - 1) +
                                         index.ToString();
                    if (workingStringValue[0] == '-')
                    {
                        workingStringValue = workingStringValue.Substring(1);
                    }
                }
                else
                {
                    index = Array.IndexOf(negativeChars, lastCharFromWorkingStringValue);
                    if (index > -1)
                    {
                        workingStringValue = workingStringValue.Substring(0, workingStringValue.Length - 1) +
                                             index.ToString();
                        if (workingStringValue[0] != '-')
                        {
                            workingStringValue = "-" + workingStringValue;
                        }
                    }
                }
            }

            return workingStringValue;
        }

        internal bool ValidateContext(object value)
        {
            Context workingContext = (Context)_context;
            if (value == null && workingContext.IsRequired)
            {
                throw new ArgumentNullException("value", "The field, " + workingContext.FieldDisplayName + ", is required.");
            }
            if (value != null && value.GetType() != workingContext.DataType)
            {
                throw new InvalidCastException("The field, " + workingContext.FieldDisplayName + ", must be of type " + workingContext.DataType.Name + " but is of type " + value.GetType().Name + ".");
            }
            return true;
        }
    }
}