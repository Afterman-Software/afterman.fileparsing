using System.Collections.Generic;

/// <summary>
/// Extension methods for IDictionary<string, object>.
/// </summary>
public static class IDictionaryStringObjectExtensions
{
	/// <summary>
	/// Returns a query-string-compatible string for the a IDictionary<string, object>.
	/// </summary>
	/// <param name="dict">The IDictionary<string, object> from which to create a query-string.</param>
	/// <param name="keyValueSep">Separator character between keys and values. Defaults to '='.</param>
	/// <param name="keyValuePairSep">Separator character between key/value pairs. Defaults to '&'.</param>
	/// <returns>A query-string-compatible string.</returns>
	public static string ToQString(this IDictionary<string, object> dict, char keyValueSep = '=', char keyValuePairSep = '&')
	{
		string result = "";
		foreach (KeyValuePair<string, object> kvp in dict)
		{
			if (kvp.Key != null && kvp.Value != null)
				result += kvp.Key + keyValueSep + kvp.Value.ToString() + keyValuePairSep;
		}
		result = result.TrimEnd(keyValuePairSep);
		return result;
	}
}