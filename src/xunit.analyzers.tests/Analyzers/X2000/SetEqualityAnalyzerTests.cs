using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SetEqualityAnalyzer>;

public class SetEqualityAnalyzerTests
{
	const string customSet = @"
using System.Collections;
using System.Collections.Generic;

public class MySet : ISet<int> {
	public int Count => throw new System.NotImplementedException();
	public bool IsReadOnly => throw new System.NotImplementedException();

	public bool Add(int item) => throw new System.NotImplementedException();
	public void Clear() => throw new System.NotImplementedException();
	public bool Contains(int item) => throw new System.NotImplementedException();
	public void CopyTo(int[] array, int arrayIndex) => throw new System.NotImplementedException();
	public void ExceptWith(IEnumerable<int> other) => throw new System.NotImplementedException();
	public IEnumerator<int> GetEnumerator() => throw new System.NotImplementedException();
	public void IntersectWith(IEnumerable<int> other) => throw new System.NotImplementedException();
	public bool IsProperSubsetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
	public bool IsProperSupersetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
	public bool IsSubsetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
	public bool IsSupersetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
	public bool Overlaps(IEnumerable<int> other) => throw new System.NotImplementedException();
	public bool Remove(int item) => throw new System.NotImplementedException();
	public bool SetEquals(IEnumerable<int> other) => throw new System.NotImplementedException();
	public void SymmetricExceptWith(IEnumerable<int> other) => throw new System.NotImplementedException();
	public void UnionWith(IEnumerable<int> other) => throw new System.NotImplementedException();
	void ICollection<int>.Add(int item) => throw new System.NotImplementedException();
	IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException();
}";

	[Theory]
	[InlineData("Equal", "List", "List")]
	[InlineData("Equal", "HashSet", "List")]
	[InlineData("Equal", "List", "HashSet")]
	[InlineData("NotEqual", "List", "List")]
	[InlineData("NotEqual", "HashSet", "List")]
	[InlineData("NotEqual", "List", "HashSet")]
	public async void ForSetWithNonSet_DoesNotTrigger(
		string method,
		string collection1Type,
		string collection2Type)
	{
		var code = @$"
using Xunit;
using System.Collections.Generic;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = new {collection1Type}<int>();
        var collection2 = new {collection2Type}<int>();

        Assert.{method}(collection1, collection2, (int e1, int e2) => true);
    }}
}}";

		await Verify.VerifyAnalyzer(code);
	}

	public class X2026_SetsMustBeComparedWithEqualityComparer
	{
		public static MatrixTheoryData<string, string, string> MethodWithCollectionCreationData =>
			new(
				new[] { "Equal", "NotEqual" },
				new[] { "new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()" },
				new[] { "new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()" }
			);

		[Theory]
		[MemberData(nameof(MethodWithCollectionCreationData))]
		public async void WithCollectionComparer_DoesNotTrigger(
			string method,
			string collection1,
			string collection2)
		{
			var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = {collection1};
        var collection2 = {collection2};

        Assert.{method}(collection1, collection2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
    }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSet });
		}

		[Theory]
		[MemberData(nameof(MethodWithCollectionCreationData))]
		public async void WithEqualityComparer_DoesNotTrigger(
			string method,
			string collection1,
			string collection2)
		{
			var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestEqualityComparer : IEqualityComparer<int>
{{
    public bool Equals(int x, int y)
    {{
        return true;
    }}

    public int GetHashCode(int obj)
    {{
        return 0;
    }}
}}

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = {collection1};
        var collection2 = {collection2};

        Assert.{method}(collection1, collection2, new TestEqualityComparer());
    }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSet });
		}

		[Theory]
		[MemberData(nameof(MethodWithCollectionCreationData))]
		public async void WithComparerLambda_Triggers(
			string method,
			string collection1,
			string collection2)
		{
			var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = {collection1};
        var collection2 = {collection2};

        Assert.{method}(collection1, collection2, (int e1, int e2) => true);
    }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit2026")
					.WithSpan(12, 9, 12, 68 + method.Length)
					.WithArguments(method);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSet }, expected);
		}

#if ROSLYN_4_2_OR_GREATER  // No C# 10 in Roslyn 3.11, so no local functions

		public static MatrixTheoryData<string, string, string, string> ComparerFunctionData() =>
			new(
				new[] { "Equal", "NotEqual" },
				new[] { "(int e1, int e2) => true", "FuncComparer", "LocalFunc", "funcDelegate" },
				new[] { "new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()" },
				new[] { "new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()" }
			);

		[Theory]
		[MemberData(nameof(ComparerFunctionData))]
		public async void WithComparerFunction_Triggers(
			string method,
			string comparerFuncSyntax,
			string collection1,
			string collection2)
		{
			var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestClass {{
    private bool FuncComparer(int obj1, int obj2)
    {{
        return true;
    }}

    private delegate bool FuncDelegate(int obj1, int obj2);

    [Fact]
    public void TestMethod() {{
        var collection1 = {collection1};
        var collection2 = {collection2};

        bool LocalFunc(int obj1, int obj2)
        {{
            return true;
        }}

        var funcDelegate = FuncComparer;

        Assert.{method}(collection1, collection2, {comparerFuncSyntax});
    }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit2026")
					.WithSpan(26, 9, 26, 44 + method.Length + comparerFuncSyntax.Length)
					.WithArguments(method);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, new[] { code, customSet }, expected);
		}

#endif

	}
}
