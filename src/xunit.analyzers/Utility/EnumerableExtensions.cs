using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
static partial class EnumerableExtensions
{
	static readonly Func<object, bool> notNullTest = x => x is not null;

	public static void AddRange<T>(
		this HashSet<T> hashSet,
		IEnumerable<T> source)
	{
		foreach (var item in source)
			hashSet.Add(item);
	}

	/// <summary>
	/// Returns <paramref name="source"/> as an enumerable of <typeparamref name="T"/> with
	/// all the <c>null</c> items removed.
	/// </summary>
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
		where T : class =>
			source.Where((Func<T?, bool>)notNullTest)!;
}
