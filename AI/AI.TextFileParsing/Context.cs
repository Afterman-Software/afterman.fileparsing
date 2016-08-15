using System;
using System.Collections.Generic;
using System.Globalization;
using AI.TextFileParsing.Enums;
using AI.TextFileParsing.Interfaces;

namespace AI.TextFileParsing
{
    [Serializable]
    public class Context : MarshalByRefObject, IContext
    {
        private readonly DataTypeEnum _dataTypeId;
        private readonly Type _dataType;
        private readonly bool _isRequired;
        private readonly bool _hasDefaultValue;
        private readonly string _defaultValue;
        private readonly string _fileTypeDescription;
        private readonly int? _substringStart;
        private readonly int? _substringLength;
        private readonly int? _fieldFormatId;
        private readonly string _fieldFormatDescription;
        private readonly string _fieldDisplayName;
        private readonly string _fieldDescription;
        private readonly string _targetTableName;
        private readonly string _targetFieldName;

        public Context
            (
                DataTypeEnum dataTypeId,
                bool isRequired,
                bool hasDefaultValue,
                string defaultValue,
                string fileTypeDescription,
                int? substringStart,
                int? substringLength,
                int? fieldFormatId,
                string fieldDisplayName,
                string fieldDescription,
                string targetTableName,
                string targetFieldName
            )
        {
            _dataTypeId = dataTypeId;
            _dataType = GetTypeFromDataType(_dataTypeId);
            _isRequired = isRequired;
            _hasDefaultValue = hasDefaultValue;
            _defaultValue = defaultValue;
            _fileTypeDescription = fileTypeDescription;
            _substringStart = substringStart;
            _substringLength = substringLength;
            _fieldFormatId = fieldFormatId;
            if (_fieldFormatId.HasValue)
            {
                if (_dataTypeId == DataTypeEnum.DecimalCurrency)
                {
                    CurrencyFormatEnum patternEnum = (CurrencyFormatEnum)fieldFormatId;
                    _fieldFormatDescription = patternEnum.GetDisplayDescription();
                }
                else if (_dataTypeId == DataTypeEnum.DecimalPercent)
                {
                    PercentFormatEnum patternEnum = (PercentFormatEnum)fieldFormatId;
                    _fieldFormatDescription = patternEnum.GetDisplayDescription();
                }
                else if (_dataTypeId == DataTypeEnum.Date)
                {
                    DateFormatEnum patternEnum = (DateFormatEnum)fieldFormatId;
                    _fieldFormatDescription = patternEnum.GetDisplayDescription();
                }
                else if (_dataTypeId == DataTypeEnum.Time)
                {
                    TimeFormatEnum patternEnum = (TimeFormatEnum)fieldFormatId;
                    _fieldFormatDescription = patternEnum.GetDisplayDescription();
                }
            }
            _fieldDisplayName = fieldDisplayName;
            _fieldDescription = fieldDescription;
            _targetTableName = targetTableName;
            _targetFieldName = targetFieldName;
        }

        public DataTypeEnum DataTypeId
        {
            get
            {
                return _dataTypeId;
            }
        }

        public Type DataType
        {
            get
            {
                return _dataType;
            }
        }

        public bool IsRequired
        {
            get
            {
                return _isRequired;
            }
        }

        public bool HasDefaultValue
        {
            get
            {
                return _hasDefaultValue;
            }
        }

        public string DefaultValue
        {
            get
            {
                return _defaultValue;
            }
        }

        public string FileTypeDescription
        {
            get
            {
                return _fileTypeDescription;
            }
        }

        public int? SubstringStart
        {
            get
            {
                return _substringStart;
            }
        }

        public int? SubstringLength
        {
            get
            {
                return _substringLength;
            }
        }

        public int? FieldFormatId
        {
            get
            {
                return _fieldFormatId;
            }
        }

        public string FieldFormatDescription
        {
            get
            {
                return _fieldFormatDescription;
            }
        }

        public string FieldDisplayName
        {
            get
            {
                return _fieldDisplayName;
            }
        }

        public string FieldDescription
        {
            get
            {
                return _fieldDescription;
            }
        }

        public string TargetTableName
        {
            get
            {
                return _targetTableName;
            }
        }

        public string TargetFieldName
        {
            get
            {
                return _targetFieldName;
            }
        }



        private Type GetTypeFromDataType(DataTypeEnum dataTypeId)
        {
            switch (dataTypeId)
            {
                case DataTypeEnum.String:
                    return typeof(string);
                case DataTypeEnum.Integer:
                    return typeof(int);
                case DataTypeEnum.DecimalCurrency:
                case DataTypeEnum.DecimalPercent:
                    return typeof(decimal);
                case DataTypeEnum.Date:
                case DataTypeEnum.Time:
                    return typeof(DateTime);
                case DataTypeEnum.Boolean:
                    return typeof(bool);
            }
            return typeof(string);
        }



        public static DateTime? ParseDate(string value, int? fieldFormatId)
        {
            if (!fieldFormatId.HasValue)
                return null;

            if (String.IsNullOrWhiteSpace(value))
                return null;

            if (String.IsNullOrWhiteSpace(value.Trim().Trim('-', '/', '0')))
                return null;

            if (value.Trim().IndexOf(' ') > 0)
            {
                value = value.Trim();
                int position = value.IndexOf(' ');
                value = value.Substring(0, position);
            }

            DateFormatEnum patternEnum = (DateFormatEnum)fieldFormatId;
            string pattern = patternEnum.GetDisplayDescription();

            var parseExactFormats = new List<string>()
            {
                "MM/dd/yyyy",
				"MM/dd/yy",
				"MM-dd-yy",
				"MM-dd-yyyy",
				"MMM-dd-yy",
				"MMM-dd-yyyy",
				"MMM-yy",
				"MMM-yyyy",
				"M/d/yyyy",
				"yyyy-MM-dd",
				"yyMMdd",
				"ddMMyy",
				"MMddyyyy",
				"yyddMM",
				"yyyyddMM",
				"yyyyMMdd",
				"dd-MMM-yy",
				"MMddyy",
				"Mddyy",
				"Mddyyyy",
            };

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            DateTime iResult;
            int jDays;
            int iYear;
            switch (pattern)
            {
                case "MM/dd/yyyy":
                case "MM/dd/yy":
                case "MM-dd-yy":
                case "MM-dd-yyyy":
                case "MMM-dd-yy":
                case "MMM-dd-yyyy":
                case "MMM-yy":
                case "MMM-yyyy":
                case "M/d/yyyy":
                case "yyyy-MM-dd":
                case "yyMMdd":
                case "ddMMyy":
                case "MMddyyyy":
                case "yyddMM":
                case "yyyyddMM":
                case "yyyyMMdd":
                case "dd-MMM-yy":
                case "MMddyy":
                case "Mddyy":
                case "Mddyyyy":
                    var dt = DateTime.MinValue;
                    var result = DateTime.TryParseExact(value, pattern, cultureInfo, DateTimeStyles.AssumeLocal, out dt);
                    if (result)
                        return dt;
                    foreach (var ptrn in parseExactFormats)
                    {
                        result = DateTime.TryParseExact(value, ptrn, cultureInfo, DateTimeStyles.AssumeLocal, out dt);
                        if (result)
                            return dt;
                    }
                    return null;
                //throw new InvalidOperationException("DateTime could not be parsed");
                //return DateTime.ParseExact(value, pattern, cultureInfo);
                case "#yyyy-MM-dd#":
                    return DateTime.ParseExact(value.Trim('#'), pattern.Trim('#'), cultureInfo);
                case "MMddyy/Mddyy":
                case "MMddyyyy/Mddyyyy":
                    if (value.Length == 5)
                    {
                        return DateTime.ParseExact(value, "Mddyy", cultureInfo);
                    }
                    if (value.Length == 6)
                    {
                        return DateTime.ParseExact(value, "MMddyy", cultureInfo);
                    }
                    if (value.Length == 7)
                    {
                        return DateTime.ParseExact(value, "Mddyyyy", cultureInfo);
                    }
                    if (value.Length == 8)
                    {
                        return DateTime.ParseExact(value, "MMddyyyy", cultureInfo);
                    }
                    return null;
                case "yyyyJJJ":
                    jDays = Convert.ToInt32(value.Substring(4, 3));
                    iYear = Convert.ToInt32(value.Substring(0, 4));
                    iResult = new JulianCalendar().AddDays(new DateTime(iYear, 1, 1), jDays - 1);
                    return iResult;
                case "yyJJJ":
                    jDays = Convert.ToInt32(value.Substring(2, 3));
                    iYear = Convert.ToInt32(value.Substring(0, 2));
                    iResult = new JulianCalendar().AddDays(new DateTime(iYear, 1, 1), jDays - 1);
                    return iResult;
                case "JJJyy":
                    jDays = Convert.ToInt32(value.Substring(0, 3));
                    iYear = Convert.ToInt32(value.Substring(3, 2));
                    iResult = new JulianCalendar().AddDays(new DateTime(iYear, 1, 1), jDays - 1);
                    return iResult;
                case "JJJyyyy":
                    jDays = Convert.ToInt32(value.Substring(0, 3));
                    iYear = Convert.ToInt32(value.Substring(3, 4));
                    iResult = new JulianCalendar().AddDays(new DateTime(iYear, 1, 1), jDays - 1);
                    return iResult;
                default:
                    return DateTime.Parse(value, cultureInfo);
            }
        }

        public static DateTime? ParseTime(string value, int? fieldFormatId)
        {
            if (!fieldFormatId.HasValue)
                return null;

            if (String.IsNullOrWhiteSpace(value))
                return null;

            if (String.IsNullOrWhiteSpace(value.Trim().Trim(':', '0')))
                return null;


            TimeFormatEnum patternEnum = (TimeFormatEnum)fieldFormatId;
            string pattern = patternEnum.GetDisplayDescription();

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            switch (pattern)
            {
                case "Unknown Format":
                    return DateTime.Parse(value, cultureInfo);
                default:
                    return DateTime.ParseExact(value, pattern, cultureInfo);
            }
        }

        public static decimal DivideByImpliedDecimalRate(decimal value, int? fieldFormatId)
        {
            if (!fieldFormatId.HasValue)
                return value;

            switch (fieldFormatId.Value)
            {
                case 1:
                case 2:
                case 4:
                    {
                        return value / 100;
                    }
                case 3:
                case 5:
                    {
                        return value;
                    }
                case 6:
                    {
                        return value / 10;
                    }
                case 7:
                    {
                        return value / 100;
                    }
                case 8:
                    {
                        return value / 1000;
                    }
                case 9:
                    {
                        return value / 10000;
                    }
                case 10:
                    {
                        return value / 100000;
                    }
                case 11:
                    {
                        return value / 1000000;
                    }
            }
            return value;
        }

        public static decimal DivideByImpliedDecimalCurrency(decimal value, int? fieldFormatId)
        {
            if (!fieldFormatId.HasValue)
                return value;

            return value / (decimal)Math.Pow(10, fieldFormatId.Value);
        }

        public static bool? ParseBoolean(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            value = value.ToUpper();

            if (value == "T" || value == "TRUE" || value == "1" || value == "Y" || value == "YES")
                return true;
            return false;
        }
    }
}