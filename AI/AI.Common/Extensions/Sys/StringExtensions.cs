/// <summary>
/// Extension methods for Strings.
/// </summary>
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static class AiStringExtensions
{
    public static string StripHtml(this string source)
    {
        return Regex.Replace(source, "<.*?>", string.Empty);
    }

    public static string StripHtmlWithReturn(string source)
    {
        return Regex.Replace(source, "<.*?>", string.Empty);
    }

    public static string TruncateForDisplay(this string inputString)
    {

        return TruncateForDisplay(inputString, 100);
    }

    public static string TruncateForDisplayWithReturn(string inputString)
    {

        return TruncateForDisplayWithReturn(inputString, 100);
    }

    public static string TruncateForDisplay(this string inputString, int length)
    {
        if (inputString == null)
            return null;
        if (inputString.Length <= length)
            return inputString;
        return inputString.Substring(0, length) + "...";
    }

    public static string TruncateForDisplayWithReturn(string inputString, int length)
    {
        if (inputString == null)
            return null;
        if (inputString.Length <= length)
            return inputString;
        return inputString.Substring(0, length) + "...";
    }

    /// <summary>
    /// Given a directory path, it will return a directory info, creating the directory if it does not already exist.
    /// </summary>
    /// <param name="directory"></param>
    /// <returns></returns>
    public static DirectoryInfo EnsureDirectoryExists(this string directory)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        return new DirectoryInfo(directory);

    }

    /// <summary>
    /// Trims a given String from the beginning of a String instance.
    /// </summary>
    /// <param name="inputString">The String instance on which to trim.</param>
    /// <param name="trimString">The String to trim from the start of the String instance.</param>
    /// <returns>A start-trimmed String, if the trimString is found, otherwise, the original String.</returns>
    /// <remarks>The trimString parameter is case-sensitive.</remarks>
    public static string TrimStart(this string inputString, string trimString)
    {
        if (inputString.StartsWith(trimString))
            return inputString.Substring(trimString.Length);
        else
            return inputString;
    }

    /// <summary>
    /// Trims a given String from the end of a String instance.
    /// </summary>
    /// <param name="inputString">The String instance on which to trim.</param>
    /// <param name="trimString">The String to trim from the end of the String instance.</param>
    /// <returns>An end-trimmed String, if the trimString is found, otherwise, the original String.</returns>
    /// <remarks>The trimString parameter is case-sensitive.</remarks>
    public static string TrimEnd(this string inputString, string trimString)
    {
        if (inputString.EndsWith(trimString))
            return inputString.Substring(0, inputString.Length - trimString.Length);
        else
            return inputString;
    }

    /// <summary>
    /// Trims a given String from the start and end of a String instance.
    /// </summary>
    /// <param name="inputString">The String instance on which to trim.</param>
    /// <param name="trimString">The String to trim from the start and end of the String instance.</param>
    /// <returns>A start-trimmed and end-trimmed String, if the trimString is found, otherwise, the original String.</returns>
    /// <remarks>The trimString parameter is case-sensitive.</remarks>
    public static string Trim(this string inputString, string trimString)
    {
        inputString = inputString.TrimStart(trimString);
        inputString = inputString.TrimEnd(trimString);
        return inputString;
    }

    /// <summary>
    /// Returns a String of the given length, either right or left padded if necessary with the given pad character.
    /// If the given length exceeds the String instance length, it is truncated.
    /// </summary>
    /// <param name="inputString">The String instance to make a fixed length.</param>
    /// <param name="length">The desired length of the resultant String.</param>
    /// <param name="padChar">The pad character to use as needed for padding. The default is the space character.</param>
    /// <param name="leftPad">A Boolean value indicating whether to pad to the left of the String instance. The default is to pad to the right.</param>
    /// <returns>A String of the given length, padded or truncated as necessary.</returns>
    public static string ToFixedLength(this string inputString, int length, char padChar = ' ', bool leftPad = false)
    {
        if (inputString.Length < length)
        {
            if (leftPad)
            {
                return new string(padChar, length - inputString.Length) + inputString;
            }
            else
            {
                return inputString + new string(padChar, length - inputString.Length);
            }
        }
        if (inputString.Length > length)
        {
            return inputString.Substring(0, length);
        }
        return inputString;
    }

    public static string ToFixedLengthWithReturn(string inputString, int length, char padChar = ' ', bool leftPad = false)
    {
        if (inputString.Length < length)
        {
            if (leftPad)
            {
                return new string(padChar, length - inputString.Length) + inputString;
            }
            else
            {
                return inputString + new string(padChar, length - inputString.Length);
            }
        }
        if (inputString.Length > length)
        {
            return inputString.Substring(0, length);
        }
        return inputString;
    }

    /// <summary>
    /// Returns a String, truncated if necessary, of a maximum length.
    /// </summary>
    /// <param name="inputString">The String instance to make a fit length.</param>
    /// <param name="maxLength">The maximum length of the resultant String.</param>
    /// <returns>A String of a maximum length, truncated if necessary, or the original String instance if it is shorter than the maximum length.</returns>
    public static string ToFitLength(this string inputString, int maxLength)
    {
        if (inputString.Length < maxLength)
        {
            return inputString;
        }
        else
        {
            return inputString.Substring(0, maxLength);
        }
    }

    public static string ToFitLengthWithReturn(string inputString, int maxLength)
    {
        if (inputString.Length < maxLength)
        {
            return inputString;
        }
        else
        {
            return inputString.Substring(0, maxLength);
        }
    }

    /// <summary>
    /// Returns a value from a query-string-compatible string, or null if not found.
    /// </summary>
    /// <param name="inputString">The query-string-compatible to search.</param>
    /// <param name="key">The key to search for in the query-string-compatible inputString.</param>
    /// <param name="keyValueSep">Separator character between keys and values. Defaults to '='.</param>
    /// <param name="keyValuePairSep">Separator character between key/value pairs. Defaults to '&'.</param>
    /// <returns>A value from a query-string-compatible string, or null if not found.</returns>
    public static string GetQStringPart(this string inputString, string key, char keyValueSep = '=', char keyValuePairSep = '&')
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.FromQString(inputString, keyValueSep, keyValuePairSep);
        if (dict.ContainsKey(key))
            return dict[key];
        else
            return null;
    }

    public static string GetQStringPartWithReturn(string inputString, string key, char keyValueSep = '=', char keyValuePairSep = '&')
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        dict.FromQString(inputString, keyValueSep, keyValuePairSep);
        if (dict.ContainsKey(key))
            return dict[key];
        else
            return null;
    }
}