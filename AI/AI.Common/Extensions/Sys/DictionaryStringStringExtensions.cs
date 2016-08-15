using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for Dictionary<string, string>.
/// </summary>
public static class DictionaryStringStringExtensions
{
	/// <summary>
	/// Adds a Dictionary<string, string> to another Dictionary<string, string>. Duplicates are overwritten.
	/// </summary>
	/// <param name="dict">The Dictionary<string, string> to which to add.</param>
	/// <param name="dictToAdd">The Dictionary<string, string> to add to the parent Dictionary<string, string>.</param>
	public static void AddRange(this Dictionary<string, string> dict, Dictionary<string, string> dictToAdd)
	{
		foreach (KeyValuePair<string, string> kvp in dictToAdd)
		{
			if (dict.ContainsKey(kvp.Key))
				dict[kvp.Key] = kvp.Value;
			else
				dict.Add(kvp.Key, kvp.Value);
		}
	}

	/// <summary>
	/// Returns a query-string-compatible string for the a Dictionary<string, string>.
	/// </summary>
	/// <param name="dict">The Dictionary<string, string> from which to create a query-string.</param>
	/// <param name="keyValueSep">Separator character between keys and values. Defaults to '='.</param>
	/// <param name="keyValuePairSep">Separator character between key/value pairs. Defaults to '&'.</param>
	/// <returns>A query-string-compatible string.</returns>
	public static string ToQString(this Dictionary<string, string> dict, char keyValueSep = '=', char keyValuePairSep = '&')
	{
		string result = "";
		foreach (KeyValuePair<string, string> kvp in dict)
		{
			if (kvp.Key != null && kvp.Value != null)
				result += kvp.Key + keyValueSep + kvp.Value + keyValuePairSep;
		}
		result = result.TrimEnd(keyValuePairSep);
		return result;
	}

	/// <summary>
	/// Returns a Dictionary<string, string> from a query-string-compatible string.
	/// </summary>
	/// <param name="dict">The Dictionary<string, string> to build from the a query-string.</param>
	/// <param name="qString">The query-string-compatible string to convert.</param>
	/// <param name="keyValueSep">Separator character between keys and values. Defaults to '='.</param>
	/// <param name="keyValuePairSep">Separator character between key/value pairs. Defaults to '&'.</param>
	/// <returns>A Dictionary<string, string> object.</returns>
	public static Dictionary<string, string> FromQString(this Dictionary<string, string> dict, string qString, char keyValueSep = '=', char keyValuePairSep = '&')
	{
		dict.Clear();
		string[] keyValuePairs = qString.Split(new char[] { keyValuePairSep }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string keyValuePair in keyValuePairs)
		{
			string[] keyValue = keyValuePair.Split(new char[] { keyValueSep }, StringSplitOptions.RemoveEmptyEntries);
			if (keyValue.Length == 1)
				dict.Add(keyValue[0], "");
			else if (keyValue.Length == 2)
				dict.Add(keyValue[0], keyValue[1]);
		}
		return dict;
	}
}