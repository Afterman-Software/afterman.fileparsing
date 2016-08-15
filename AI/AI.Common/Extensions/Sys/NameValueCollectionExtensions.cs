using System.Collections.Specialized;

/// <summary>
/// Extension methods for NameValueCollection.
/// </summary>
public static class NameValueCollectionExtensions
{
	/// <summary>
	/// Returns a query-string-compatible string for the a NameValueCollection.
	/// </summary>
	/// <param name="nvc">The NameValueCollection from which to create a query-string.</param>
	/// <param name="keyValueSep">Separator character between keys and values. Defaults to '='.</param>
	/// <param name="keyValuePairSep">Separator character between key/value pairs. Defaults to '&'.</param>
	/// <returns>A query-string-compatible string.</returns>
	public static string ToQString(this NameValueCollection nvc, char keyValueSep = '=', char keyValuePairSep = '&')
	{
		string result = "";
		foreach (string key in nvc.Keys)
		{
			result += key + keyValueSep + nvc[key] + keyValuePairSep;
		}
		result = result.TrimEnd(keyValuePairSep);
		return result;
	}
}