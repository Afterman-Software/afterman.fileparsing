//borrowed from Microsoft.VisualBasic so that TextQualifiers can be overridden

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AI.TextFileParsing.Parsers
{
    /// <summary>Provides methods and properties for parsing structured text files.</summary>
    /// <filterpriority>1</filterpriority>
    public class DelimitedTextFileParser : ITextFileParser, IDisposable
    {
        private delegate int ChangeBufferFunction();
        private bool m_Disposed;
        private System.IO.TextReader m_Reader;
        private string[] m_CommentTokens;
        private long m_LineNumber;
        private bool m_EndOfData;
        private string m_ErrorLine;
        private long m_ErrorLineNumber;
        private Microsoft.VisualBasic.FileIO.FieldType m_TextFieldType;
        private int[] m_FieldWidths;
        private string[] m_Delimiters;
        private int[] m_FieldWidthsCopy;
        private string[] m_DelimitersCopy;
        private Regex m_DelimiterRegex;
        private Regex m_DelimiterWithEndCharsRegex;
        private int[] m_WhitespaceCodes;
        private Regex m_BeginQuotesRegex;
        private Regex m_WhiteSpaceRegEx;
        private bool m_TrimWhiteSpace;
        private int m_Position;
        private int m_PeekPosition;
        private int m_CharsRead;
        private bool m_NeedPropertyCheck;
        private char[] m_Buffer;
        private int m_LineLength;
        private bool m_HasFieldsEnclosedInQuotes;
        private string m_SpaceChars;
        private int m_MaxLineSize;
        private int m_MaxBufferSize;
        private bool m_LeaveOpen;
        private const RegexOptions REGEX_OPTIONS = RegexOptions.CultureInvariant;
        private const int DEFAULT_BUFFER_LENGTH = 4096;
        private const int DEFAULT_BUILDER_INCREASE = 10;
        private string _BEGINS_WITH_QUOTE = "\\G[{0}]*\"";
        private string _ENDING_QUOTE = "\"[{0}]*";
        private string _textQualifier = @"""";

        /// <summary>Defines comment tokens. A comment token is a string that, when placed at the beginning of a line, indicates that the line is a comment and should be ignored by the parser.</summary>
        /// <returns>A string array that contains all of the comment tokens for the <see cref="T:Microsoft.VisualBasic.FileIO.TextFieldParser" /> object.</returns>
        /// <exception cref="T:System.ArgumentException">A comment token includes white space.</exception>
        /// <filterpriority>1</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public string[] CommentTokens
        {
            get
            {
                return this.m_CommentTokens;
            }
            set
            {
                this.CheckCommentTokensForWhitespace(value);
                this.m_CommentTokens = value;
                this.m_NeedPropertyCheck = true;
            }
        }
        /// <summary>Returns True if there are no non-blank, non-comment lines between the current cursor position and the end of the file.</summary>
        /// <returns>True if there is no more data to read; otherwise, False.</returns>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public bool EndOfData
        {
            get
            {
                if (this.m_EndOfData)
                {
                    return this.m_EndOfData;
                }
                if (this.m_Reader == null | this.m_Buffer == null)
                {
                    this.m_EndOfData = true;
                    return true;
                }
                if (this.PeekNextDataLine() != null)
                {
                    return false;
                }
                this.m_EndOfData = true;
                return true;
            }
        }
        /// <summary>Returns the current line number, or returns -1 if no more characters are available in the stream.</summary>
        /// <returns>The current line number.</returns>
        /// <filterpriority>1</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public long LineNumber
        {
            get
            {
                if (this.m_LineNumber != -1L && (this.m_Reader.Peek() == -1 & this.m_Position == this.m_CharsRead))
                {
                    this.CloseReader();
                }
                return this.m_LineNumber;
            }
        }
        /// <summary>Returns the line that caused the most recent <see cref="T:Microsoft.VisualBasic.FileIO.MalformedLineException" /> exception.</summary>
        /// <returns>The line that caused the most recent <see cref="T:Microsoft.VisualBasic.FileIO.MalformedLineException" /> exception.</returns>
        /// <filterpriority>1</filterpriority>
        public string ErrorLine
        {
            get
            {
                return this.m_ErrorLine;
            }
        }
        /// <summary>Returns the number of the line that caused the most recent <see cref="T:Microsoft.VisualBasic.FileIO.MalformedLineException" /> exception.</summary>
        /// <returns>The number of the line that caused the most recent <see cref="T:Microsoft.VisualBasic.FileIO.MalformedLineException" /> exception.</returns>
        /// <filterpriority>1</filterpriority>
        public long ErrorLineNumber
        {
            get
            {
                return this.m_ErrorLineNumber;
            }
        }
        /// <summary>Indicates whether the file to be parsed is delimited or fixed-width.</summary>
        /// <returns>A <see cref="P:Microsoft.VisualBasic.FileIO.TextFieldParser.TextFieldType" /> value that indicates whether the file to be parsed is delimited or fixed-width.</returns>
        /// <filterpriority>1</filterpriority>
        private Microsoft.VisualBasic.FileIO.FieldType TextFieldType
        {
            get
            {
                return this.m_TextFieldType;
            }
            set
            {
                this.ValidateFieldTypeEnumValue(value, "value");
                this.m_TextFieldType = value;
                this.m_NeedPropertyCheck = true;
            }
        }
        /// <summary>Denotes the width of each column in the text file being parsed.</summary>
        /// <returns>An integer array that contains the width of each column in the text file that is being parsed.</returns>
        /// <exception cref="T:System.ArgumentException">A width value in any location other than the last entry of the array is less than or equal to zero.</exception>
        /// <filterpriority>1</filterpriority>
        private int[] FieldWidths
        {
            get
            {
                return this.m_FieldWidths;
            }
            set
            {
                if (value != null)
                {
                    this.ValidateFieldWidthsOnInput(value);
                    this.m_FieldWidthsCopy = (int[])value.Clone();
                }
                else
                {
                    this.m_FieldWidthsCopy = null;
                }
                this.m_FieldWidths = value;
                this.m_NeedPropertyCheck = true;
            }
        }
        /// <summary>Defines the delimiters for a text file. </summary>
        /// <returns>A string array that contains all of the field delimiters for the <see cref="T:Microsoft.VisualBasic.FileIO.TextFieldParser" /> object.</returns>
        /// <exception cref="T:System.ArgumentException">A delimiter value is set to a newline character, an empty string, or Nothing.</exception>
        /// <filterpriority>1</filterpriority>
        public string[] Delimiters
        {
            get
            {
                return this.m_Delimiters;
            }
            set
            {
                if (value != null)
                {
                    this.ValidateDelimiters(value);
                    this.m_DelimitersCopy = (string[])value.Clone();
                }
                else
                {
                    this.m_DelimitersCopy = null;
                }
                this.m_Delimiters = value;
                this.m_NeedPropertyCheck = true;
                this.m_BeginQuotesRegex = null;
            }
        }
        /// <summary>Indicates whether leading and trailing white space should be trimmed from field values.</summary>
        /// <returns>True if leading and trailing white space should be trimmed from field values; otherwise, False.</returns>
        /// <filterpriority>1</filterpriority>
        public bool TrimWhiteSpace
        {
            get
            {
                return this.m_TrimWhiteSpace;
            }
            set
            {
                this.m_TrimWhiteSpace = value;
            }
        }
        /// <summary>Denotes whether fields are enclosed in quotation marks when a delimited file is being parsed.</summary>
        /// <returns>True if fields are enclosed in quotation marks; otherwise, False.</returns>
        /// <filterpriority>1</filterpriority>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool HasFieldsEnclosedInQualifier
        {
            get
            {
                return this.m_HasFieldsEnclosedInQuotes;
            }
            set
            {
                this.m_HasFieldsEnclosedInQuotes = value;
            }
        }
        private Regex BeginQuotesRegex
        {
            get
            {
                if (this.m_BeginQuotesRegex == null)
                {
                    string pattern = string.Format(CultureInfo.InvariantCulture, _BEGINS_WITH_QUOTE, new object[]
					{
						this.WhitespacePattern
					});
                    this.m_BeginQuotesRegex = new Regex(pattern, RegexOptions.CultureInvariant);
                }
                return this.m_BeginQuotesRegex;
            }
        }
        private string EndQuotePattern
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, _ENDING_QUOTE, new object[]
				{
					this.WhitespacePattern
				});
            }
        }
        private string WhitespaceCharacters
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                int[] whitespaceCodes = this.m_WhitespaceCodes;
                checked
                {
                    for (int i = 0; i < whitespaceCodes.Length; i++)
                    {
                        int charCode = whitespaceCodes[i];
                        char c = Microsoft.VisualBasic.Strings.ChrW(charCode);
                        if (!this.CharacterIsInDelimiter(c))
                        {
                            stringBuilder.Append(c);
                        }
                    }
                    return stringBuilder.ToString();
                }
            }
        }
        private string WhitespacePattern
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                int[] whitespaceCodes = this.m_WhitespaceCodes;
                checked
                {
                    for (int i = 0; i < whitespaceCodes.Length; i++)
                    {
                        int charCode = whitespaceCodes[i];
                        char testCharacter = Microsoft.VisualBasic.Strings.ChrW(charCode);
                        if (!this.CharacterIsInDelimiter(testCharacter))
                        {
                            stringBuilder.Append("\\u" + charCode.ToString("X4", CultureInfo.InvariantCulture));
                        }
                    }
                    return stringBuilder.ToString();
                }
            }
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="path">String. The complete path of the file to be parsed.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="path" /> is an empty string.</exception>
        private DelimitedTextFileParser(string path)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            this.InitializeFromPath(path, Encoding.UTF8, true);
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="path">String. The complete path of the file to be parsed.</param>
        /// <param name="defaultEncoding">
        ///   <see cref="T:System.Text.Encoding" />. The character encoding to use if encoding is not determined from file. Default is <see cref="P:System.Text.Encoding.UTF8" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="path" /> is an empty string or <paramref name="defaultEncoding" /> is Nothing.</exception>
        private DelimitedTextFileParser(string path, Encoding defaultEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            this.InitializeFromPath(path, defaultEncoding, true);
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="path">String. The complete path of the file to be parsed.</param>
        /// <param name="defaultEncoding">
        ///   <see cref="T:System.Text.Encoding" />. The character encoding to use if encoding is not determined from file. Default is <see cref="P:System.Text.Encoding.UTF8" />.</param>
        /// <param name="detectEncoding">Boolean. Indicates whether to look for byte order marks at the beginning of the file. Default is True.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="path" /> is an empty string or <paramref name="defaultEncoding" /> is Nothing.</exception>
        private DelimitedTextFileParser(string path, Encoding defaultEncoding, bool detectEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            this.InitializeFromPath(path, defaultEncoding, detectEncoding);
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="stream">
        ///   <see cref="T:System.IO.Stream" />. The stream to be parsed.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> is Nothing.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="stream" /> cannot be read from.</exception>
        public DelimitedTextFileParser(System.IO.Stream stream)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            this.InitializeFromStream(stream, Encoding.UTF8, true);
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="stream">
        ///   <see cref="T:System.IO.Stream" />. The stream to be parsed.</param>
        /// <param name="defaultEncoding">
        ///   <see cref="T:System.Text.Encoding" />. The character encoding to use if encoding is not determined from file. Default is <see cref="P:System.Text.Encoding.UTF8" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> or <paramref name="defaultEncoding" /> is Nothing.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="stream" /> cannot be read from.</exception>
        private DelimitedTextFileParser(System.IO.Stream stream, Encoding defaultEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            this.InitializeFromStream(stream, defaultEncoding, true);
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="stream">
        ///   <see cref="T:System.IO.Stream" />. The stream to be parsed.</param>
        /// <param name="defaultEncoding">
        ///   <see cref="T:System.Text.Encoding" />. The character encoding to use if encoding is not determined from file. Default is <see cref="P:System.Text.Encoding.UTF8" />.</param>
        /// <param name="detectEncoding">Boolean. Indicates whether to look for byte order marks at the beginning of the file. Default is True.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> or <paramref name="defaultEncoding" /> is Nothing.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="stream" /> cannot be read from.</exception>
        private DelimitedTextFileParser(System.IO.Stream stream, Encoding defaultEncoding, bool detectEncoding)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            this.InitializeFromStream(stream, defaultEncoding, detectEncoding);
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="stream">
        ///   <see cref="T:System.IO.Stream" />. The stream to be parsed.</param>
        /// <param name="defaultEncoding">
        ///   <see cref="T:System.Text.Encoding" />. The character encoding to use if encoding is not determined from file. Default is <see cref="P:System.Text.Encoding.UTF8" />.</param>
        /// <param name="detectEncoding">Boolean. Indicates whether to look for byte order marks at the beginning of the file. Default is True.</param>
        /// <param name="leaveOpen">Boolean. Indicates whether to leave <paramref name="stream" /> open when the TextFieldParser object is closed. Default is False.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> or <paramref name="defaultEncoding" /> is Nothing.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="stream" /> cannot be read from.</exception>
        private DelimitedTextFileParser(System.IO.Stream stream, Encoding defaultEncoding, bool detectEncoding, bool leaveOpen)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            this.m_LeaveOpen = leaveOpen;
            this.InitializeFromStream(stream, defaultEncoding, detectEncoding);
        }
        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="reader">
        ///   <see cref="T:System.IO.TextReader" />. The <see cref="T:System.IO.TextReader" /> stream to be parsed. </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="reader" /> is Nothing.</exception>
        private DelimitedTextFileParser(System.IO.TextReader reader)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
            this.m_WhitespaceCodes = new int[]
			{
				9,
				11,
				12,
				32,
				133,
				160,
				5760,
				8192,
				8193,
				8194,
				8195,
				8196,
				8197,
				8198,
				8199,
				8200,
				8201,
				8202,
				8203,
				8232,
				8233,
				12288,
				65279
			};
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = true;
            this.m_Position = 0;
            this.m_PeekPosition = 0;
            this.m_CharsRead = 0;
            this.m_NeedPropertyCheck = true;
            this.m_Buffer = new char[4096];
            this.m_HasFieldsEnclosedInQuotes = true;
            this.m_MaxLineSize = 10000000;
            this.m_MaxBufferSize = 10000000;
            this.m_LeaveOpen = false;
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            this.m_Reader = reader;
            this.ReadToBuffer();
        }
        /// <summary>Sets the delimiters for the reader to the specified values, and sets the field type to Delimited.</summary>
        /// <param name="delimiters">Array of type String. </param>
        /// <exception cref="T:System.ArgumentException">A delimiter is zero-length.</exception>
        public void SetDelimiters(params string[] delimiters)
        {
            this.Delimiters = delimiters;
        }
        /// <summary>Sets the delimiters for the reader to the specified values.</summary>
        /// <param name="fieldWidths">Array of Integer. </param>
        private void SetFieldWidths(params int[] fieldWidths)
        {
            this.FieldWidths = fieldWidths;
        }
        /// <summary>Sets the delimiters for the reader to the specified values.</summary>
        /// <param name="fieldWidths">Array of Integer. </param>
        public void SetTextQualifier(string textQualifier)
        {
            this._textQualifier = textQualifier;
            this._BEGINS_WITH_QUOTE = @"\G[{0}]*\" + this._textQualifier + "";
            this._ENDING_QUOTE = @"\" + this._textQualifier + "[{0}]*";
        }
        /// <summary>Returns the current line as a string and advances the cursor to the next line.</summary>
        /// <returns>The current line from the file or stream.</returns>
        /// <filterpriority>1</filterpriority>
        public string ReadLine()
        {
            if (this.m_Reader == null | this.m_Buffer == null)
            {
                return null;
            }
            DelimitedTextFileParser.ChangeBufferFunction changeBuffer = new DelimitedTextFileParser.ChangeBufferFunction(this.ReadToBuffer);
            string text = this.ReadNextLine(ref this.m_Position, changeBuffer);
            if (text == null)
            {
                this.FinishReading();
                return null;
            }
            checked
            {
                this.m_LineNumber += 1L;
                return text.TrimEnd(new char[]
				{
					'\r',
					'\n'
				});
            }
        }
        /// <summary>Reads all fields on the current line, returns them as an array of strings, and advances the cursor to the next line containing data.</summary>
        /// <returns>An array of strings that contains field values for the current line.</returns>
        /// <exception cref="T:Microsoft.VisualBasic.FileIO.MalformedLineException">A field cannot be parsed by using the specified format.</exception>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public string[] ReadFields()
        {
            if (this.m_Reader == null | this.m_Buffer == null)
            {
                return null;
            }
            this.ValidateReadyToRead();
            switch (this.m_TextFieldType)
            {
                case Microsoft.VisualBasic.FileIO.FieldType.Delimited:
                    return this.ParseDelimitedLine();
                case Microsoft.VisualBasic.FileIO.FieldType.FixedWidth:
                    return this.ParseFixedWidthLine();
                default:
                    return null;
            }
        }
        /// <summary>Reads the specified number of characters without advancing the cursor.</summary>
        /// <returns>A string that contains the specified number of characters read.</returns>
        /// <param name="numberOfChars">Int32. Number of characters to read. Required. </param>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="numberOfChars" /> is less than 0.</exception>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        private string PeekChars(int numberOfChars)
        {
            if (numberOfChars <= 0)
            {
                throw new ArgumentException("Number of characters must be positive", "numberOfChars");
            }
            if (this.m_Reader == null | this.m_Buffer == null)
            {
                return null;
            }
            if (this.m_EndOfData)
            {
                return null;
            }
            string text = this.PeekNextDataLine();
            if (text == null)
            {
                this.m_EndOfData = true;
                return null;
            }
            text = text.TrimEnd(new char[]
			{
				'\r',
				'\n'
			});
            if (text.Length < numberOfChars)
            {
                return text;
            }
            StringInfo stringInfo = new StringInfo(text);
            return stringInfo.SubstringByTextElements(0, numberOfChars);
        }
        /// <summary>Reads the remainder of the text file and returns it as a string.</summary>
        /// <returns>The remaining text from the file or stream.</returns>
        /// <filterpriority>1</filterpriority>
        private string ReadToEnd()
        {
            if (this.m_Reader == null | this.m_Buffer == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder(this.m_Buffer.Length);
            stringBuilder.Append(this.m_Buffer, this.m_Position, checked(this.m_CharsRead - this.m_Position));
            stringBuilder.Append(this.m_Reader.ReadToEnd());
            this.FinishReading();
            return stringBuilder.ToString();
        }
        /// <summary>Closes the current TextFieldParser object.</summary>
        /// <filterpriority>1</filterpriority>
        public void Close()
        {
            this.CloseReader();
        }
        /// <summary>Releases resources used by the <see cref="T:Microsoft.VisualBasic.FileIO.TextFieldParser" /> object.</summary>
        public void Dispose()
        {
            this.Dispose(true);
        }
        /// <summary>Releases resources used by the <see cref="T:Microsoft.VisualBasic.FileIO.TextFieldParser" /> object.</summary>
        /// <param name="disposing">Boolean. True releases both managed and unmanaged resources; False releases only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.m_Disposed)
                {
                    this.Close();
                }
                this.m_Disposed = true;
            }
        }
        private void CloseReader()
        {
            this.FinishReading();
            if (this.m_Reader != null)
            {
                if (!this.m_LeaveOpen)
                {
                    this.m_Reader.Close();
                }
                this.m_Reader = null;
            }
        }
        private void ValidateFieldTypeEnumValue(Microsoft.VisualBasic.FileIO.FieldType value, string paramName)
        {
            if (value < Microsoft.VisualBasic.FileIO.FieldType.Delimited || value > Microsoft.VisualBasic.FileIO.FieldType.FixedWidth)
            {
                throw new InvalidEnumArgumentException(paramName, (int)value, typeof(Microsoft.VisualBasic.FileIO.FieldType));
            }
        }
        private void FinishReading()
        {
            this.m_LineNumber = -1L;
            this.m_EndOfData = true;
            this.m_Buffer = null;
            this.m_DelimiterRegex = null;
            this.m_BeginQuotesRegex = null;
        }
        private void InitializeFromPath(string path, Encoding defaultEncoding, bool detectEncoding)
        {
            if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(path, "", false) == 0)
            {
                throw new ArgumentNullException("path");
            }
            if (defaultEncoding == null)
            {
                throw new ArgumentNullException("defaultEncoding");
            }
            string path2 = this.ValidatePath(path);
            System.IO.FileStream stream = new System.IO.FileStream(path2, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
            this.m_Reader = new System.IO.StreamReader(stream, defaultEncoding, detectEncoding);
            this.ReadToBuffer();
        }
        private void InitializeFromStream(System.IO.Stream stream, Encoding defaultEncoding, bool detectEncoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException("Stream not readable", "stream");
            }
            if (defaultEncoding == null)
            {
                throw new ArgumentNullException("defaultEncoding");
            }
            this.m_Reader = new System.IO.StreamReader(stream, defaultEncoding, detectEncoding);
            this.ReadToBuffer();
        }
        private string ValidatePath(string path)
        {
            string text = NormalizeFilePath(path, "path");
            if (!System.IO.File.Exists(text))
            {
                throw new System.IO.FileNotFoundException("File not found", text);
            }
            return text;
        }
        private bool IgnoreLine(string line)
        {
            if (line == null)
            {
                return false;
            }
            string text = line; //.Trim();
            if (text.Length == 0)
            {
                return true;
            }
            checked
            {
                if (this.m_CommentTokens != null)
                {
                    string[] commentTokens = this.m_CommentTokens;
                    for (int i = 0; i < commentTokens.Length; i++)
                    {
                        string text2 = commentTokens[i];
                        if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(text2, "", false) != 0)
                        {
                            if (text.StartsWith(text2, StringComparison.Ordinal))
                            {
                                return true;
                            }
                            if (line.StartsWith(text2, StringComparison.Ordinal))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }
        private int ReadToBuffer()
        {
            this.m_Position = 0;
            int num = this.m_Buffer.Length;
            if (num > 4096)
            {
                num = 4096;
                this.m_Buffer = new char[checked(num - 1 + 1)];
            }
            this.m_CharsRead = this.m_Reader.Read(this.m_Buffer, 0, num);
            return this.m_CharsRead;
        }
        private int SlideCursorToStartOfBuffer()
        {
            checked
            {
                if (this.m_Position > 0)
                {
                    int num = this.m_Buffer.Length;
                    int num2 = this.m_CharsRead - this.m_Position;
                    char[] array = new char[num - 1 + 1];
                    Array.Copy(this.m_Buffer, this.m_Position, array, 0, num2);
                    int num3 = this.m_Reader.Read(array, num2, num - num2);
                    this.m_CharsRead = num2 + num3;
                    this.m_Position = 0;
                    this.m_Buffer = array;
                    return num3;
                }
                return 0;
            }
        }
        private int IncreaseBufferSize()
        {
            this.m_PeekPosition = this.m_CharsRead;
            checked
            {
                int num = this.m_Buffer.Length + 4096;
                if (num > this.m_MaxBufferSize)
                {
                    throw new InvalidOperationException("Buffer exceeded maximum size");
                }
                char[] array = new char[num - 1 + 1];
                Array.Copy(this.m_Buffer, array, this.m_Buffer.Length);
                int num2 = this.m_Reader.Read(array, this.m_Buffer.Length, 4096);
                this.m_Buffer = array;
                this.m_CharsRead += num2;
                return num2;
            }
        }
        private string ReadNextDataLine()
        {
            DelimitedTextFileParser.ChangeBufferFunction changeBuffer = new DelimitedTextFileParser.ChangeBufferFunction(this.ReadToBuffer);
            checked
            {
                string text;
                do
                {
                    text = this.ReadNextLine(ref this.m_Position, changeBuffer);
                    this.m_LineNumber += 1L;
                }
                while (this.IgnoreLine(text));
                if (text == null)
                {
                    this.CloseReader();
                }
                return text;
            }
        }
        private string PeekNextDataLine()
        {
            DelimitedTextFileParser.ChangeBufferFunction changeBuffer = new DelimitedTextFileParser.ChangeBufferFunction(this.IncreaseBufferSize);
            this.SlideCursorToStartOfBuffer();
            this.m_PeekPosition = 0;
            string text;
            do
            {
                text = this.ReadNextLine(ref this.m_PeekPosition, changeBuffer);
            }
            while (this.IgnoreLine(text));
            return text;
        }
        private string ReadNextLine(ref int Cursor, DelimitedTextFileParser.ChangeBufferFunction ChangeBuffer)
        {
            if (Cursor == this.m_CharsRead && ChangeBuffer() == 0)
            {
                return null;
            }
            StringBuilder stringBuilder = null;
            checked
            {
                int i;
                char value;
                while (true)
                {
                    int arg_23_0 = Cursor;
                    int num = this.m_CharsRead - 1;
                    for (i = arg_23_0; i <= num; i++)
                    {
                        value = this.m_Buffer[i];
                        if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(value), "\r", false) == 0 | Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(value), "\n", false) == 0)
                        {
                            goto Block_3;
                        }
                    }
                    int num2 = this.m_CharsRead - Cursor;
                    if (stringBuilder == null)
                    {
                        stringBuilder = new StringBuilder(num2 + 10);
                    }
                    stringBuilder.Append(this.m_Buffer, Cursor, num2);
                    if (ChangeBuffer() <= 0)
                    {
                        goto Block_12;
                    }
                }
            Block_3:
                if (stringBuilder != null)
                {
                    stringBuilder.Append(this.m_Buffer, Cursor, i - Cursor + 1);
                }
                else
                {
                    stringBuilder = new StringBuilder(i + 1);
                    stringBuilder.Append(this.m_Buffer, Cursor, i - Cursor + 1);
                }
                Cursor = i + 1;
                if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(value), "\r", false) == 0)
                {
                    if (Cursor < this.m_CharsRead)
                    {
                        if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(this.m_Buffer[Cursor]), "\n", false) == 0)
                        {
                            Cursor++;
                            stringBuilder.Append("\n");
                        }
                    }
                    else
                    {
                        if (ChangeBuffer() > 0 && Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(this.m_Buffer[Cursor]), "\n", false) == 0)
                        {
                            Cursor++;
                            stringBuilder.Append("\n");
                        }
                    }
                }
                return stringBuilder.ToString();
            Block_12:
                return stringBuilder.ToString();
            }
        }
        private string[] ParseDelimitedLine()
        {
            string text = this.ReadNextDataLine();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            checked
            {
                long num = this.m_LineNumber - 1L;
                int i = 0;
                List<string> list = new List<string>();
                int endOfLineIndex = this.GetEndOfLineIndex(text);
                while (i <= endOfLineIndex)
                {
                    Match match = null;
                    bool flag = false;
                    if (this.m_HasFieldsEnclosedInQuotes)
                    {
                        match = this.BeginQuotesRegex.Match(text, i);
                        flag = match.Success;
                    }
                    if (flag)
                    {
                        i = match.Index + match.Length;
                        QuoteDelimitedFieldBuilder quoteDelimitedFieldBuilder = new QuoteDelimitedFieldBuilder(this.m_DelimiterWithEndCharsRegex, this.m_SpaceChars);
                        quoteDelimitedFieldBuilder.BuildField(text, i);
                        if (quoteDelimitedFieldBuilder.MalformedLine)
                        {
                            this.m_ErrorLine = text.TrimEnd(new char[]
							{
								'\r',
								'\n'
							});
                            this.m_ErrorLineNumber = num;
                            throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Malformed delimited line at line " + num.ToString());
                        }
                        string text3;
                        if (!quoteDelimitedFieldBuilder.FieldFinished)
                        {
                            do
                            {
                                int length = text.Length;
                                string text2 = this.ReadNextDataLine();
                                if (text2 == null)
                                {
                                    goto Block_6;
                                }
                                if (text.Length + text2.Length > this.m_MaxLineSize)
                                {
                                    goto Block_7;
                                }
                                text += text2;
                                endOfLineIndex = this.GetEndOfLineIndex(text);
                                quoteDelimitedFieldBuilder.BuildField(text, length);
                                if (quoteDelimitedFieldBuilder.MalformedLine)
                                {
                                    goto Block_8;
                                }
                            }
                            while (!quoteDelimitedFieldBuilder.FieldFinished);
                            text3 = quoteDelimitedFieldBuilder.Field;
                            i = quoteDelimitedFieldBuilder.Index + quoteDelimitedFieldBuilder.DelimiterLength;
                            goto IL_286;
                        Block_6:
                            this.m_ErrorLine = text.TrimEnd(new char[]
							{
								'\r',
								'\n'
							});
                            this.m_ErrorLineNumber = num;
                            throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Malformed delimited line at line " + num.ToString());
                        Block_7:
                            this.m_ErrorLine = text.TrimEnd(new char[]
							{
								'\r',
								'\n'
							});
                            this.m_ErrorLineNumber = num;
                            throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Delimited line max size exceeded at line " + num.ToString());
                        Block_8:
                            this.m_ErrorLine = text.TrimEnd(new char[]
							{
								'\r',
								'\n'
							});
                            this.m_ErrorLineNumber = num;
                            throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Malformed delimited line at line " + num.ToString());
                        }
                        text3 = quoteDelimitedFieldBuilder.Field;
                        i = quoteDelimitedFieldBuilder.Index + quoteDelimitedFieldBuilder.DelimiterLength;
                    IL_286:
                        if (this.m_TrimWhiteSpace)
                        {
                            text3 = text3.Trim();
                        }
                        list.Add(text3);
                    }
                    else
                    {
                        Match match2 = this.m_DelimiterRegex.Match(text, i);
                        string text3;
                        if (!match2.Success)
                        {
                            text3 = text.Substring(i).TrimEnd(new char[]
							{
								'\r',
								'\n'
							});
                            if (this.m_TrimWhiteSpace)
                            {
                                text3 = text3.Trim();
                            }
                            list.Add(text3);
                            break;
                        }
                        text3 = text.Substring(i, match2.Index - i);
                        if (this.m_TrimWhiteSpace)
                        {
                            text3 = text3.Trim();
                        }
                        list.Add(text3);
                        i = match2.Index + match2.Length;
                    }
                }
                list.Add(text);
                return list.ToArray();
            }
        }
        private string[] ParseFixedWidthLine()
        {
            string text = this.ReadNextDataLine();
            if (text == null)
            {
                return null;
            }
            text = text.TrimEnd(new char[]
			{
				'\r',
				'\n'
			});
            StringInfo line = new StringInfo(text);
            checked
            {
                this.ValidateFixedWidthLine(line, this.m_LineNumber - 1L);
                int num = 0;
                int num2 = this.m_FieldWidths.Length - 1;
                string[] array = new string[num2 + 1];
                int arg_5C_0 = 0;
                int num3 = num2;
                for (int i = arg_5C_0; i <= num3; i++)
                {
                    array[i] = this.GetFixedWidthField(line, num, this.m_FieldWidths[i]);
                    num += this.m_FieldWidths[i];
                }
                return array;
            }
        }
        private string GetFixedWidthField(StringInfo Line, int Index, int FieldLength)
        {
            string text;
            if (FieldLength > 0)
            {
                text = Line.SubstringByTextElements(Index, FieldLength);
            }
            else
            {
                if (Index >= Line.LengthInTextElements)
                {
                    text = string.Empty;
                }
                else
                {
                    text = Line.SubstringByTextElements(Index).TrimEnd(new char[]
					{
						'\r',
						'\n'
					});
                }
            }
            if (this.m_TrimWhiteSpace)
            {
                return text.Trim();
            }
            return text;
        }
        private int GetEndOfLineIndex(string Line)
        {
            int length = Line.Length;
            if (length == 1)
            {
                return length;
            }
            checked
            {
                if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(Line[length - 2]), "\r", false) == 0 | Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(Line[length - 2]), "\n", false) == 0)
                {
                    return length - 2;
                }
                if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(Line[length - 1]), "\r", false) == 0 | Microsoft.VisualBasic.CompilerServices.Operators.CompareString(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(Line[length - 1]), "\n", false) == 0)
                {
                    return length - 1;
                }
                return length;
            }
        }
        private void ValidateFixedWidthLine(StringInfo Line, long LineNumber)
        {
            if (Line.LengthInTextElements < this.m_LineLength)
            {
                this.m_ErrorLine = Line.String;
                this.m_ErrorLineNumber = checked(this.m_LineNumber - 1L);
                throw new Microsoft.VisualBasic.FileIO.MalformedLineException("Malformed fixed width line at line " + LineNumber.ToString());
            }
        }
        private void ValidateFieldWidths()
        {
            if (this.m_FieldWidths == null)
            {
                throw new InvalidOperationException("Field widths are nothing");
            }
            if (this.m_FieldWidths.Length == 0)
            {
                throw new InvalidOperationException("Field widths are nothing");
            }
            checked
            {
                int num = this.m_FieldWidths.Length - 1;
                this.m_LineLength = 0;
                int arg_4C_0 = 0;
                int num2 = num - 1;
                for (int i = arg_4C_0; i <= num2; i++)
                {
                    this.m_LineLength += this.m_FieldWidths[i];
                }
                if (this.m_FieldWidths[num] > 0)
                {
                    this.m_LineLength += this.m_FieldWidths[num];
                }
            }
        }
        private void ValidateFieldWidthsOnInput(int[] Widths)
        {
            checked
            {
                int num = Widths.Length - 1;
                int arg_0B_0 = 0;
                int num2 = num - 1;
                for (int i = arg_0B_0; i <= num2; i++)
                {
                    if (Widths[i] < 1)
                    {
                        throw new ArgumentException("Field widths must be positive", "Widths");
                    }
                }
            }
        }
        private void ValidateAndEscapeDelimiters()
        {
            if (this.m_Delimiters == null)
            {
                throw new ArgumentException("Delimiters are nothing", "delimiterArray");
            }
            if (this.m_Delimiters.Length == 0)
            {
                throw new ArgumentException("Delimiters are nothing", "delimiterArray");
            }
            int num = this.m_Delimiters.Length;
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            stringBuilder2.Append(this.EndQuotePattern + "(");
            int arg_8B_0 = 0;
            checked
            {
                int num2 = num - 1;
                for (int i = arg_8B_0; i <= num2; i++)
                {
                    if (this.m_Delimiters[i] != null)
                    {
                        if (this.m_HasFieldsEnclosedInQuotes && this.m_Delimiters[i].IndexOf(this._textQualifier) > -1)
                        {
                            throw new InvalidOperationException("Invalid delimiters");
                        }
                        string str = Regex.Escape(this.m_Delimiters[i]);
                        stringBuilder.Append(str + "|");
                        stringBuilder2.Append(str + "|");
                    }
                }
                this.m_SpaceChars = this.WhitespaceCharacters;
                this.m_DelimiterRegex = new Regex(stringBuilder.ToString(0, stringBuilder.Length - 1), RegexOptions.CultureInvariant);
                stringBuilder.Append("\r|\n");
                this.m_DelimiterWithEndCharsRegex = new Regex(stringBuilder.ToString(), RegexOptions.CultureInvariant);
                stringBuilder2.Append("\r|\n)|\"$");
            }
        }
        private void ValidateReadyToRead()
        {
            checked
            {
                if (this.m_NeedPropertyCheck | this.ArrayHasChanged())
                {
                    switch (this.m_TextFieldType)
                    {
                        case Microsoft.VisualBasic.FileIO.FieldType.Delimited:
                            this.ValidateAndEscapeDelimiters();
                            break;
                        case Microsoft.VisualBasic.FileIO.FieldType.FixedWidth:
                            this.ValidateFieldWidths();
                            break;
                    }
                    if (this.m_CommentTokens != null)
                    {
                        string[] commentTokens = this.m_CommentTokens;
                        for (int i = 0; i < commentTokens.Length; i++)
                        {
                            string text = commentTokens[i];
                            if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(text, "", false) != 0 && (this.m_HasFieldsEnclosedInQuotes & this.m_TextFieldType == Microsoft.VisualBasic.FileIO.FieldType.Delimited) && string.Compare(text.Trim(), this._textQualifier, StringComparison.Ordinal) == 0)
                            {
                                throw new InvalidOperationException("Invalid comment");
                            }
                        }
                    }
                    this.m_NeedPropertyCheck = false;
                }
            }
        }
        private void ValidateDelimiters(string[] delimiterArray)
        {
            if (delimiterArray == null)
            {
                return;
            }
            checked
            {
                for (int i = 0; i < delimiterArray.Length; i++)
                {
                    string text = delimiterArray[i];
                    if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(text, "", false) == 0)
                    {
                        throw new ArgumentException("Delimiters are nothing", "delimiterArray");
                    }
                    if (text.IndexOfAny(new char[]
					{
						'\r',
						'\n'
					}) > -1)
                    {
                        throw new ArgumentException("End characters in delimiters", "delimiterArray");
                    }
                }
            }
        }
        private bool ArrayHasChanged()
        {
            checked
            {
                switch (this.m_TextFieldType)
                {
                    case Microsoft.VisualBasic.FileIO.FieldType.Delimited:
                        {
                            if (this.m_Delimiters == null)
                            {
                                return false;
                            }
                            int lowerBound = this.m_DelimitersCopy.GetLowerBound(0);
                            int upperBound = this.m_DelimitersCopy.GetUpperBound(0);
                            int arg_44_0 = lowerBound;
                            int num = upperBound;
                            for (int i = arg_44_0; i <= num; i++)
                            {
                                if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(this.m_Delimiters[i], this.m_DelimitersCopy[i], false) != 0)
                                {
                                    return true;
                                }
                            }
                            break;
                        }
                    case Microsoft.VisualBasic.FileIO.FieldType.FixedWidth:
                        {
                            if (this.m_FieldWidths == null)
                            {
                                return false;
                            }
                            int lowerBound = this.m_FieldWidthsCopy.GetLowerBound(0);
                            int upperBound = this.m_FieldWidthsCopy.GetUpperBound(0);
                            int arg_95_0 = lowerBound;
                            int num2 = upperBound;
                            for (int j = arg_95_0; j <= num2; j++)
                            {
                                if (this.m_FieldWidths[j] != this.m_FieldWidthsCopy[j])
                                {
                                    return true;
                                }
                            }
                            break;
                        }
                }
                return false;
            }
        }
        private void CheckCommentTokensForWhitespace(string[] tokens)
        {
            if (tokens == null)
            {
                return;
            }
            checked
            {
                for (int i = 0; i < tokens.Length; i++)
                {
                    string input = tokens[i];
                    if (this.m_WhiteSpaceRegEx.IsMatch(input))
                    {
                        throw new ArgumentException("Whitespace in tokens", "tokens");
                    }
                }
            }
        }
        private bool CharacterIsInDelimiter(char testCharacter)
        {
            string[] delimiters = this.m_Delimiters;
            checked
            {
                for (int i = 0; i < delimiters.Length; i++)
                {
                    string text = delimiters[i];
                    if (text.IndexOf(testCharacter) > -1)
                    {
                        return true;
                    }
                }
                return false;
            }
        }



        //borrowed internals from Microsoft.VisualBasic
        private string GetLongPath(string FullPath)
        {
            string result;
            try
            {
                if (System.IO.Path.IsPathRooted(FullPath))
                {
                    result = FullPath;
                }
                else
                {
                    System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(FullPath).Parent;
                    if (System.IO.File.Exists(FullPath))
                    {
                        result = directoryInfo.GetFiles(System.IO.Path.GetFileName(FullPath))[0].FullName;
                    }
                    else
                    {
                        if (System.IO.Directory.Exists(FullPath))
                        {
                            result = directoryInfo.GetDirectories(System.IO.Path.GetFileName(FullPath))[0].FullName;
                        }
                        else
                        {
                            result = FullPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is ArgumentException))
                {
                    if (!(ex is ArgumentNullException))
                    {
                        if (!(ex is System.IO.PathTooLongException))
                        {
                            if (!(ex is NotSupportedException))
                            {
                                if (!(ex is System.IO.DirectoryNotFoundException))
                                {
                                    if (!(ex is System.Security.SecurityException))
                                    {
                                        if (!(ex is UnauthorizedAccessException))
                                        {
                                            throw;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                result = FullPath;
            }
            return result;
        }
        private string RemoveEndingSeparator(string path)
        {
            if (System.IO.Path.IsPathRooted(path) && System.IO.Path.Equals(System.IO.Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
            return path.TrimEnd(new char[]
				{
					System.IO.Path.DirectorySeparatorChar,
					System.IO.Path.AltDirectorySeparatorChar
				});
        }
        private string NormalizePath(string path)
        {
            return GetLongPath(RemoveEndingSeparator(System.IO.Path.GetFullPath(path)));
        }
        private string NormalizeFilePath(string path, string paramName)
        {
            CheckFilePathTrailingSeparator(path, paramName);
            return NormalizePath(path);
        }
        private void CheckFilePathTrailingSeparator(string path, string paramName)
        {
            if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(path, "", false) == 0)
            {
                throw new ArgumentNullException(paramName);
            }
            if (path.EndsWith(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(System.IO.Path.DirectorySeparatorChar), StringComparison.Ordinal) | path.EndsWith(Microsoft.VisualBasic.CompilerServices.Conversions.ToString(System.IO.Path.AltDirectorySeparatorChar), StringComparison.Ordinal))
            {
                throw new ArgumentException("File path exception", paramName);
            }
        }
    }



    internal class QuoteDelimitedFieldBuilder
    {
        private StringBuilder m_Field;
        private bool m_FieldFinished;
        private int m_Index;
        private int m_DelimiterLength;
        private Regex m_DelimiterRegex;
        private string m_SpaceChars;
        private bool m_MalformedLine;
        public bool FieldFinished
        {
            get
            {
                return this.m_FieldFinished;
            }
        }
        public string Field
        {
            get
            {
                return this.m_Field.ToString();
            }
        }
        public int Index
        {
            get
            {
                return this.m_Index;
            }
        }
        public int DelimiterLength
        {
            get
            {
                return this.m_DelimiterLength;
            }
        }
        public bool MalformedLine
        {
            get
            {
                return this.m_MalformedLine;
            }
        }
        public QuoteDelimitedFieldBuilder(Regex DelimiterRegex, string SpaceChars)
        {
            this.m_Field = new StringBuilder();
            this.m_DelimiterRegex = DelimiterRegex;
            this.m_SpaceChars = SpaceChars;
        }
        public void BuildField(string Line, int StartAt)
        {
            this.m_Index = StartAt;
            int length = Line.Length;
            checked
            {
                while (this.m_Index < length)
                {
                    if (Line[this.m_Index] == '"')
                    {
                        if (this.m_Index + 1 == length)
                        {
                            this.m_FieldFinished = true;
                            this.m_DelimiterLength = 1;
                            this.m_Index++;
                            return;
                        }
                        if (!(this.m_Index + 1 < Line.Length & Line[this.m_Index + 1] == '"'))
                        {
                            Match match = this.m_DelimiterRegex.Match(Line, this.m_Index + 1);
                            int num;
                            if (!match.Success)
                            {
                                num = length - 1;
                            }
                            else
                            {
                                num = match.Index - 1;
                            }
                            int arg_CB_0 = this.m_Index + 1;
                            int num2 = num;
                            for (int i = arg_CB_0; i <= num2; i++)
                            {
                                if (this.m_SpaceChars.IndexOf(Line[i]) < 0)
                                {
                                    this.m_MalformedLine = true;
                                    return;
                                }
                            }
                            this.m_DelimiterLength = 1 + num - this.m_Index;
                            if (match.Success)
                            {
                                this.m_DelimiterLength += match.Length;
                            }
                            this.m_FieldFinished = true;
                            return;
                        }
                        this.m_Field.Append('"');
                        this.m_Index += 2;
                    }
                    else
                    {
                        this.m_Field.Append(Line[this.m_Index]);
                        this.m_Index++;
                    }
                }
            }
        }
    }
}