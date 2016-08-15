using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AI.TextFileParsing.Parsers
{
    public class FixedWidthTextFileParser : ITextFileParser, IDisposable
    {
        private bool m_Disposed;
        private TextReader m_Reader;
        private string[] m_CommentTokens;
        private long m_LineNumber;
        private bool m_EndOfData;
        private string m_ErrorLine;
        private long m_ErrorLineNumber;
        private List<Tuple<int, int>> m_FieldWidths;
        private int m_LineLength;
        private Regex m_WhiteSpaceRegEx;
        private bool m_TrimWhiteSpace;
        private bool m_LeaveOpen;

        /// <summary>Initializes a new instance of the TextFieldParser class.</summary>
        /// <param name="stream">
        ///   <see cref="T:System.IO.Stream" />. The stream to be parsed.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="stream" /> is Nothing.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="stream" /> cannot be read from.</exception>
        public FixedWidthTextFileParser(System.IO.Stream stream)
        {
            this.m_CommentTokens = new string[0];
            this.m_LineNumber = 1L;
            this.m_EndOfData = false;
            this.m_ErrorLine = "";
            this.m_ErrorLineNumber = -1L;
            this.m_FieldWidths = new List<Tuple<int, int>>();
            this.m_LineLength = 0;
            this.m_WhiteSpaceRegEx = new Regex("\\s", RegexOptions.CultureInvariant);
            this.m_TrimWhiteSpace = false;
            this.m_LeaveOpen = false;
            this.InitializeFromStream(stream, Encoding.UTF8, true);
        }
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
                if (this.m_Reader == null)
                {
                    this.m_EndOfData = true;
                    return true;
                }
                if (this.m_Reader.Peek() != -1)
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
                if (this.m_LineNumber != -1L && this.m_Reader.Peek() == -1)
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
        public long LineLength
        {
            get
            {
                return this.m_LineLength;
            }
        }
        /// <summary>Sets the line length for the reader to the specified value.</summary>
        /// <param name="fieldWidths">Array of Integer. </param>
        public void SetLineLength(int lineLength)
        {
            this.m_LineLength = lineLength;
        }
        /// <summary>Denotes the start and width of each column in the text file being parsed.</summary>
        /// <returns>A List<Tuple<int, int>> that contains the start and width of each column in the text file that is being parsed.</returns>
        /// <exception cref="T:System.ArgumentException">A width value in any location other than the last entry of the array is less than or equal to zero.</exception>
        /// <filterpriority>1</filterpriority>
        public List<Tuple<int, int>> FieldWidths
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
                }
                this.m_FieldWidths = value;
            }
        }
        public void AddFieldWidth(int start, int width)
        {
            this.m_FieldWidths.Add(Tuple.Create<int, int>(start, width));
            this.ValidateFieldWidthsOnInput(this.m_FieldWidths);
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
            if (this.m_Reader == null)
            {
                return null;
            }
            this.ValidateReadyToRead();

            string text = this.ReadNextDataLine();
            if (text == null)
            {
                this.FinishReading();
                return null;
            }
            string[] results = new string[] { text };
            try
            {
                results = this.ParseFixedWidthLine(text);
                Array.Resize(ref results, results.Length + 1);
                results[results.Length - 1] = text;
            }
            catch
            {
                //TODO: swallow these errors?
            }
            return results;
        }
        private string[] ParseFixedWidthLine(string text)
        {
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
                int num = this.m_FieldWidths.Count - 1;
                string[] array = new string[num + 1];
                for (int i = 0; i <= num; i++)
                {
                    try
                    {
                        array[i] = this.GetFixedWidthField(line, this.m_FieldWidths[i]);
                    }
                    catch (Exception ex)
                    {
                        array[i] = "{{BADPARSE}}:" + ex.Message;
                    }
                }
                return array;
            }
        }
        private string GetFixedWidthField(StringInfo Line, Tuple<int, int> FieldLength)
        {
            string text;
            if (FieldLength.Item2 > 0)
            {
                text = Line.SubstringByTextElements(FieldLength.Item1 - 1, FieldLength.Item2);
            }
            else
            {
                if (FieldLength.Item1 - 1 >= Line.LengthInTextElements)
                {
                    text = string.Empty;
                }
                else
                {
                    text = Line.SubstringByTextElements(FieldLength.Item1 - 1).TrimEnd(new char[]
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
        /// <summary>Returns the current line as a string and advances the cursor to the next line.</summary>
        /// <returns>The current line from the file or stream.</returns>
        /// <filterpriority>1</filterpriority>
        public string ReadLine()
        {
            if (this.m_Reader == null)
            {
                return null;
            }
            string text = this.m_Reader.ReadLine();
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
        private string ReadNextDataLine()
        {
            checked
            {
                string text;
                do
                {
                    text = this.m_Reader.ReadLine();
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
        /// <summary>Closes the current TextFieldParser object.</summary>
        /// <filterpriority>1</filterpriority>
        public void Close()
        {
            this.CloseReader();
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

        private void ValidateReadyToRead()
        {
            checked
            {
                this.ValidateFieldWidths();
                if (this.m_CommentTokens != null)
                {
                    string[] commentTokens = this.m_CommentTokens;
                    for (int i = 0; i < commentTokens.Length; i++)
                    {
                        string text = commentTokens[i];
                        if (Microsoft.VisualBasic.CompilerServices.Operators.CompareString(text, "", false) != 0)
                        {
                            throw new InvalidOperationException("Invalid comment");
                        }
                    }
                }
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

        }



        private void FinishReading()
        {
            this.m_LineNumber = -1L;
            this.m_EndOfData = true;
        }
        private void ValidateFixedWidthLine(StringInfo Line, long LineNumber)
        {
            if (this.m_LineLength <= 0)
                return;

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
            if (this.m_FieldWidths.Count == 0)
            {
                throw new InvalidOperationException("Field widths are nothing");
            }
        }
        private void ValidateFieldWidthsOnInput(List<Tuple<int, int>> Widths)
        {
            checked
            {
                int num = Widths.Count - 1;
                for (int i = 0; i <= num; i++)
                {
                    if (Widths[i].Item1 < 1 || Widths[i].Item2 < 1)
                    {
                        throw new ArgumentException("Field starts and widths must be positive", "Widths");
                    }
                }
            }
        }
    }
}