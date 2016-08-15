using System;
using System.Collections.Generic;

/// <summary>
/// Extension methods for generic IDictionary objects.
/// </summary>
public static class IDictionaryGenericExtensions
{
	/// <summary>
	/// Adds an IEnumerable of KeyValuePairs to a generic IDictionary instance.
	/// </summary>
	/// <typeparam name="TKey">The type of the Dictionary Key.</typeparam>
	/// <typeparam name="TValue">The type of the Dictionary Value.</typeparam>
	/// <param name="destination">The generic IDictionary instance to which to add an IEnumerable of KeyValuePairs.</param>
	/// <param name="values">The IEnumerable of KeyValuePairs to add to the generic IDictionary instance.</param>
	/// <remarks>If a Key from the IEnumerable of KeyValuePairs exists in the generic IDictionary instance, an exception will be thrown.</remarks>
	public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> destination, IEnumerable<KeyValuePair<TKey, TValue>> values)
	{
		if (destination == null)
			throw new ArgumentNullException("destination");
		if (values == null)
			throw new ArgumentNullException("values");

		foreach (KeyValuePair<TKey, TValue> pair in values)
		{
			destination.Add(pair);
		}
	}

	/// <summary>
	/// Adds an IEnumerable of KeyValuePairs to a generic IDictionary instance.
	/// </summary>
	/// <typeparam name="TKey">The type of the Dictionary Key.</typeparam>
	/// <typeparam name="TValue">The type of the Dictionary Value.</typeparam>
	/// <param name="destination">The generic IDictionary instance to which to add an IEnumerable of KeyValuePairs.</param>
	/// <param name="values">The IEnumerable of KeyValuePairs to add to the generic IDictionary instance.</param>
	/// <remarks>If a Key from the IEnumerable of KeyValuePairs exists in the generic IDictionary instance, the generic IDictionary instance value will be replaced.</remarks>
	public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> destination, IEnumerable<KeyValuePair<TKey, TValue>> values)
	{
		if (destination == null)
			throw new ArgumentNullException("destination");
		if (values == null)
			throw new ArgumentNullException("values");

		foreach (KeyValuePair<TKey, TValue> pair in values)
		{
			destination[pair.Key] = pair.Value;
		}
	}
}