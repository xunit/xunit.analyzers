using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
static partial class EnumerableExtensions
{
	static readonly Func<object, bool> notNullTest = x => x is not null;

	public static void Add<TKey, TValue>(
		this Dictionary<TKey, List<TValue>> dictionary,
		TKey key,
		TValue value)
			where TKey : notnull
	{
		if (!dictionary.TryGetValue(key, out var list))
		{
			list = [];
			dictionary[key] = list;
		}

		list.Add(value);
	}

	public static void AddRange<T>(
		this HashSet<T> hashSet,
		IEnumerable<T> source)
	{
		Guard.ArgumentNotNull(hashSet);
		Guard.ArgumentNotNull(source);

		foreach (var item in source)
			hashSet.Add(item);
	}

	/// <summary>
	/// Returns <paramref name="source"/> as an enumerable of <typeparamref name="T"/> with
	/// all the <c>null</c> items removed.
	/// </summary>
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
		where T : class =>
			Guard.ArgumentNotNull(source).Where((Func<T?, bool>)notNullTest)!;
}
