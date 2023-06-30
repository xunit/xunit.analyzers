using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class AssertEqualShouldNotBeUsedForCollectionSizeCheckTests
{
	public static TheoryData<string> CollectionsWithExceptionThrowingGetEnumeratorMethod = new()
	{
		"new System.ArraySegment<int>().Count",
	};

	public static TheoryData<string> Collections = new()
	{
		"new int[0].Length",
		"new System.Collections.ArrayList().Count",
		"new System.Collections.Generic.List<int>().Count",
		"new System.Collections.Generic.HashSet<int>().Count",
		"System.Collections.Immutable.ImmutableArray.Create<int>().Length",
		"new System.Collections.ObjectModel.Collection<int>().Count",
		"new System.Collections.Generic.List<int>().AsReadOnly().Count",
		"System.Linq.Enumerable.Empty<int>().Count()",
	};

	public static TheoryData<string, int> CollectionsWithUnsupportedSize = new()
	{
		{ "new int[0].Length", -1 },
		{ "new System.Collections.ArrayList().Count", -2 },
		{ "new System.Collections.Generic.List<int>().Count", 2 },
		{ "new System.Collections.Generic.HashSet<int>().Count", 3 },
		{ "System.Collections.Immutable.ImmutableArray.Create<int>().Length", 42 },
		{ "new System.Collections.ObjectModel.Collection<int>().Count", 13 },
		{ "new System.Collections.Generic.List<int>().AsReadOnly().Count", 2 },
		{ "System.Linq.Enumerable.Empty<int>().Count()", 354 },
	};

	public static TheoryData<string> CollectionInterfaces = new()
	{
		"ICollection",
		"ICollection<string>",
		"IReadOnlyCollection<string>",
	};

	[Theory]
	[MemberData(nameof(CollectionsWithExceptionThrowingGetEnumeratorMethod))]
	public async void DoesNotFindWarningForCollectionsWithExceptionThrowingGetEnumeratorMethod(string collection)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.NotEqual(0, {collection});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async void FindsWarningForEmptyCollectionSizeCheck(string collection)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal(0, {collection});
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 32 + collection.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Empty);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(CollectionInterfaces))]
	public async void FindsWarningForCollectionInterface_Empty(string @interface)
	{
		var source = $@"
using System.Collections;
using System.Collections.Generic;

class TestClass {{
    void TestMethod() {{
        {@interface} collection = null;
        Xunit.Assert.Equal(0, collection.Count);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 48)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Empty);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(CollectionInterfaces))]
	public async void FindsWarningForCollectionInterface_Single(string @interface)
	{
		var source = $@"
using System.Collections;
using System.Collections.Generic;

class TestClass {{
    void TestMethod() {{
        {@interface} collection = null;
        Xunit.Assert.Equal(1, collection.Count);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 48)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Single);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async void FindsWarningForNonEmptyCollectionSizeCheck(string collection)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.NotEqual(0, {collection});
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 35 + collection.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async void FindsWarningForSingleItemCollectionSizeCheck(string collection)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal(1, {collection});
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 32 + collection.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Single);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindsWarningForSymbolDeclaringTypeHasZeroArity_ImplementsICollectionOfT()
	{
		var source = @"
using System.Collections;
using System.Collections.Generic;
using Xunit;

class IntCollection : ICollection<int> {
    public int Count { get { throw null; } }
    public bool IsReadOnly { get { throw null; } }
    public void Add(int item) { throw null; }
    public void Clear() { throw null; }
    public bool Contains(int item) { throw null; }
    public void CopyTo(int[] array, int arrayIndex) { throw null; }
    public IEnumerator<int> GetEnumerator() { throw null; }
    public bool Remove(int item) { throw null; }
    IEnumerator IEnumerable.GetEnumerator() { throw null; }
}

class TestClass {
    void TestMethod() {
        Assert.Equal(1, new IntCollection().Count);
    }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(20, 9, 20, 51)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Single);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async void DoesNotFindWarningForNonSingleItemCollectionSizeCheck(string collection)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.NotEqual(1, {collection});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(CollectionsWithUnsupportedSize))]
	public async void DoesNotFindWarningForUnsupportedCollectionSizeCheck(
		string collection,
		int size)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal({size}, {collection});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(CollectionsWithUnsupportedSize))]
	public async void DoesNotFindWarningForUnsupportedNonEqualCollectionSizeCheck(
		string collection,
		int size)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.NotEqual({size}, {collection});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotCrashForSymbolDeclaringTypeHasDifferentArityThanICollection_Zero()
	{
		var source = @"
using System.Collections.Generic;

interface IIntCollection : ICollection<int> {
    new int Count { get; }
}

class TestClass {
    void TestMethod() {
        Xunit.Assert.Equal(1, ((IIntCollection)null).Count);
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotCrashForSymbolDeclaringTypeHasDifferentArityThanICollection_Two()
	{
		var source = @"
using System.Collections.Generic;

interface IDictionary2<K, V> : ICollection<KeyValuePair<K, V>> {
    new int Count { get; }
}

class TestClass {
    void TestMethod() {
        Xunit.Assert.Equal(1, ((IDictionary2<int, int>)null).Count);
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotCrash_ForNonIntArguments()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.Equal('b', new int[0].Length);
    }
}";

		await Verify.VerifyAnalyzer(source);
	}
}
