// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.CodeAnalysis.Testing.Verifiers;

public class XunitVerifier : IVerifier
{
	public XunitVerifier() :
		this(ImmutableStack<string>.Empty)
	{ }

	protected XunitVerifier(ImmutableStack<string> context) =>
		Context = context ?? throw new ArgumentNullException(nameof(context));

	protected ImmutableStack<string> Context { get; }

	protected virtual string CreateMessage(string? message)
	{
		foreach (var frame in Context)
			message = "Context: " + frame + Environment.NewLine + message;

		return message ?? string.Empty;
	}

	void IVerifier.Empty<T>(
		string collectionName,
		IEnumerable<T> collection)
	{
		var tracker = CollectionTracker<T>.Wrap(collection);
		using var enumerator = tracker.GetEnumerator();

		if (enumerator.MoveNext())
			throw EmptyException.ForNamedNonEmptyCollection(tracker.FormatStart(), collectionName);
	}

	void IVerifier.Equal<T>(
		T expected,
		T actual,
		string? message)
	{
		if (EqualityComparer<T>.Default.Equals(expected, actual))
			return;

		if (message is null && Context.IsEmpty)
			throw EqualException.ForMismatchedValues(ArgumentFormatter.Format(expected), ArgumentFormatter.Format(actual));
		else
			throw EqualException.ForMismatchedValuesWithMessage(expected, actual, CreateMessage(message));
	}

	[DoesNotReturn]
	void IVerifier.Fail(string? message) =>
		Assert.Fail(message is null ? "<no message provided>" : CreateMessage(message));

	void IVerifier.False(
		[DoesNotReturnIf(true)] bool assert,
		string? message)
	{
		if (message is null && Context.IsEmpty)
			Assert.False(assert);
		else
			Assert.False(assert, CreateMessage(message));
	}

	void IVerifier.LanguageIsSupported(string language) =>
		Assert.False(language != LanguageNames.CSharp && language != LanguageNames.VisualBasic, CreateMessage($"Unsupported Language: '{language}'"));

	void IVerifier.NotEmpty<T>(
		string collectionName,
		IEnumerable<T> collection)
	{
		using var enumerator = collection.GetEnumerator();

		if (!enumerator.MoveNext())
			throw NotEmptyException.ForNamedNonEmptyCollection(collectionName);
	}

	IVerifier IVerifier.PushContext(string context)
	{
		Assert.IsAssignableFrom<XunitVerifier>(this);

		return new XunitVerifier(Context.Push(context));
	}

	void IVerifier.SequenceEqual<T>(
		IEnumerable<T> expected,
		IEnumerable<T> actual,
		IEqualityComparer<T>? equalityComparer,
		string? message)
	{
		var comparer = new SequenceEqualEnumerableEqualityComparer<T>(equalityComparer);
		var areEqual = comparer.Equals(expected, actual);

		if (!areEqual)
			throw EqualException.ForMismatchedValuesWithMessage(expected, actual, CreateMessage(message));
	}

	void IVerifier.True(
		[DoesNotReturnIf(false)] bool assert,
		string? message)
	{
		if (message is null && Context.IsEmpty)
			Assert.True(assert);
		else
			Assert.True(assert, CreateMessage(message));
	}

	sealed class SequenceEqualEnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>?>
	{
		readonly IEqualityComparer<T> itemEqualityComparer;

		public SequenceEqualEnumerableEqualityComparer(IEqualityComparer<T>? itemEqualityComparer) =>
			this.itemEqualityComparer = itemEqualityComparer ?? EqualityComparer<T>.Default;

		public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (x is null || y is null)
				return false;

			return x.SequenceEqual(y, itemEqualityComparer);
		}

		public int GetHashCode(IEnumerable<T>? obj)
		{
			if (obj is null)
				return 0;

			// From System.Tuple
			//
			// The suppression is required due to an invalid contract in IEqualityComparer<T>
			// https://github.com/dotnet/runtime/issues/30998
			return obj
				.Select(item => itemEqualityComparer.GetHashCode(item!))
				.Aggregate(
					0,
					(aggHash, nextHash) => ((aggHash << 5) + aggHash) ^ nextHash
				);
		}
	}
}
