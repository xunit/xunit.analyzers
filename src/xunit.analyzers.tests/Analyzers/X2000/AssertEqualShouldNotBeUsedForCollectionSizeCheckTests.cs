using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class AssertEqualShouldNotBeUsedForCollectionSizeCheckTests
{
	public static TheoryData<string> AllowedCollections = new()
	{
		"new System.ArraySegment<int>().Count",
		"Microsoft.Extensions.Primitives.StringValues.Empty.Count",
	};

	public static TheoryData<string> DisallowedCollections = new()
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

	public static TheoryData<string> DisallowedCollectionInterfaces = new()
	{
		"ICollection",
		"ICollection<string>",
		"IReadOnlyCollection<string>",
	};

	[Theory]
	[MemberData(nameof(AllowedCollections))]
	public async void AllowedCollection_DoesNotTrigger(string collection)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal(0, {collection});
        Xunit.Assert.Equal(1, {collection});
        Xunit.Assert.NotEqual(0, {collection});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(AllowedCollections))]
	[MemberData(nameof(DisallowedCollections))]
	public async void AllowedCheck_DoesNotTrigger(string collection)
	{
		// Anything that's non-zero for Equal/NotEqual and non-one for Equal is allowed
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal(2, {collection});
        Xunit.Assert.NotEqual(1, {collection});
        Xunit.Assert.NotEqual(2, {collection});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}


	[Theory]
	[MemberData(nameof(DisallowedCollections))]
	public async void InvalidCheckWithConcreteType_Triggers(string collection)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal(0, {collection});
        Xunit.Assert.Equal(1, {collection});
        Xunit.Assert.NotEqual(0, {collection});
    }}
}}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 32 + collection.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify
				.Diagnostic()
				.WithSpan(7, 9, 7, 32 + collection.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 35 + collection.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(DisallowedCollectionInterfaces))]
	public async void InvalidCheckWithInterfaceType_Triggers(string @interface)
	{
		var source = $@"
using System.Collections;
using System.Collections.Generic;

class TestClass {{
    void TestMethod() {{
        {@interface} collection = null;
        Xunit.Assert.Equal(0, collection.Count);
        Xunit.Assert.Equal(1, collection.Count);
        Xunit.Assert.NotEqual(0, collection.Count);
    }}
}}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 48)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify
				.Diagnostic()
				.WithSpan(9, 9, 9, 48)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify
				.Diagnostic()
				.WithSpan(10, 9, 10, 51)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void InvalidCheckWithCustomNonGenericCollection_Triggers()
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
        Assert.Equal(0, new IntCollection().Count);
        Assert.Equal(1, new IntCollection().Count);
        Assert.NotEqual(0, new IntCollection().Count);
    }
}";
		var expected = new[]
		{
			Verify
				.Diagnostic()
				.WithSpan(20, 9, 20, 51)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify
				.Diagnostic()
				.WithSpan(21, 9, 21, 51)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify
				.Diagnostic()
				.WithSpan(22, 9, 22, 54)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void OverridingCountMethod_DoesNotTrigger()
	{
		var source = @"
using System.Collections.Generic;

interface IIntCollection : ICollection<int> {
    new int Count { get; }
}

interface ICustomCollection<T> : ICollection<T> {
    new int Count { get; }
}

interface ICustomDictionary<K, V> : ICollection<KeyValuePair<K, V>> {
    new int Count { get; }
}

class TestClass {
    void TestMethod() {
        Xunit.Assert.Equal(1, ((IIntCollection)null).Count);
        Xunit.Assert.Equal(1, ((ICustomCollection<int>)null).Count);
        Xunit.Assert.Equal(1, ((ICustomDictionary<int, int>)null).Count);
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
