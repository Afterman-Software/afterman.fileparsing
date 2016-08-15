
using AI.TextFileParsing.Enums;
using AI.TextFileParsing.Interfaces;
using AI.TextFileParsing.ParsingDefinitions;
namespace AI.TextFileParsing.Parsers
{
    public static class OnDemandDelimitedTextFileParser
    {
        public static string[] ReadFields(string myline)
        {
            //string text = mylinel
            //if (string.IsNullOrWhiteSpace(text))
            //{
            //    return null;
            //}
            //checked
            //{
            //    long num = this.m_LineNumber - 1L;
            //    int i = 0;
            //    List<string> list = new List<string>();
            //    int endOfLineIndex = this.GetEndOfLineIndex(text);
            //    while (i <= endOfLineIndex)
            //    {
            //        Match match = null;
            //        bool flag = false;
            //        if (this.m_HasFieldsEnclosedInQuotes)
            //        {
            //            match = this.BeginQuotesRegex.Match(text, i);
            //            flag = match.Success;
            //        }
            //        if (flag)
            //        {
            //            i = match.Index + match.Length;
            //            QuoteDelimitedFieldBuilder quoteDelimitedFieldBuilder = new QuoteDelimitedFieldBuilder(this.m_DelimiterWithEndCharsRegex, this.m_SpaceChars);
            //            quoteDelimitedFieldBuilder.BuildField(text, i);
            //            if (quoteDelimitedFieldBuilder.MalformedLine)
            //            {
            //                this.m_ErrorLine = text.TrimEnd(new char[]
            //                {
            //                    '\r',
            //                    '\n'
            //                });
            //                this.m_ErrorLineNumber = num;
            //                throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Malformed delimited line at line " + num.ToString());
            //            }
            //            string text3;
            //            if (!quoteDelimitedFieldBuilder.FieldFinished)
            //            {
            //                do
            //                {
            //                    int length = text.Length;
            //                    string text2 = this.ReadNextDataLine();
            //                    if (text2 == null)
            //                    {
            //                        goto Block_6;
            //                    }
            //                    if (text.Length + text2.Length > this.m_MaxLineSize)
            //                    {
            //                        goto Block_7;
            //                    }
            //                    text += text2;
            //                    endOfLineIndex = this.GetEndOfLineIndex(text);
            //                    quoteDelimitedFieldBuilder.BuildField(text, length);
            //                    if (quoteDelimitedFieldBuilder.MalformedLine)
            //                    {
            //                        goto Block_8;
            //                    }
            //                }
            //                while (!quoteDelimitedFieldBuilder.FieldFinished);
            //                text3 = quoteDelimitedFieldBuilder.Field;
            //                i = quoteDelimitedFieldBuilder.Index + quoteDelimitedFieldBuilder.DelimiterLength;
            //                goto IL_286;
            //            Block_6:
            //                throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Malformed delimited line at line " + num.ToString());
            //            Block_7:
            //                throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Delimited line max size exceeded at line " + num.ToString());
            //            Block_8:
            //                throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Malformed delimited line at line " + num.ToString());
            //            }
            //            text3 = quoteDelimitedFieldBuilder.Field;
            //            i = quoteDelimitedFieldBuilder.Index + quoteDelimitedFieldBuilder.DelimiterLength;
            //        IL_286:
            //            list.Add(text3);
            //        }
            //        else
            //        {
            //            Match match2 = this.m_DelimiterRegex.Match(text, i);
            //            string text3;
            //            if (!match2.Success)
            //            {
            //                text3 = text.Substring(i).TrimEnd(new char[]
            //                {
            //                    '\r',
            //                    '\n'
            //                });
            //                if (this.m_TrimWhiteSpace)
            //                {
            //                    text3 = text3.Trim();
            //                }
            //                list.Add(text3);
            //                break;
            //            }
            //            text3 = text.Substring(i, match2.Index - i);
            //            if (this.m_TrimWhiteSpace)
            //            {
            //                text3 = text3.Trim();
            //            }
            //            list.Add(text3);
            //            i = match2.Index + match2.Length;
            //        }
            //    }
            //    list.Add(text);
            //    return list.ToArray();

            return myline.Split('\t');
        }

        public static IColumn ConvertField(RowDefinition rowDef, string value)
        {
            IContext context = new Context((DataTypeEnum)rowDef.DataType, rowDef.IsRequired, rowDef.CanSetDefault,
                               rowDef.DefaultValue, "OnDemandDelimited",
                               rowDef.ParseStartPosition,
                               rowDef.ParseLength, rowDef.FieldFormat,
                               rowDef.FieldDisplayName, rowDef.FieldDescription, rowDef.TargetTableName,
                               rowDef.TargetFieldName);

            IColumn column = new Column(value, context);
            return column;
        }
    }
}