using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SetEqualityAnalyzer>;

public class SetEqualityAnalyzerTests
{
	const string customSetAndComparer = @"
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
}

public class MyComparer : IEqualityComparer<int> {
    public bool Equals(int x, int y) => throw new System.NotImplementedException();
    public int GetHashCode(int obj) => throw new System.NotImplementedException();
}";

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

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSetAndComparer });
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

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSetAndComparer });
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

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSetAndComparer }, expected);
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

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, new[] { code, customSetAndComparer }, expected);
		}

#endif

	}

	public class X2027_SetsShouldNotBeComparedToLinearContainers
	{
		public static MatrixTheoryData<string, string> MethodAndLinearContainers =>
			new(
				new[] { "Equal", "NotEqual" },
				new[] { "new List<int>()", "new SortedSet<int>()", "new HashSet<int>().OrderBy(x => x)", "new MySet().OrderBy(x => x)" }
			);

		[Theory]
		[MemberData(nameof(MethodAndLinearContainers))]
		public async void LinearContainers_DoesNotTrigger(
			string method,
			string collection)
		{
			var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Linq;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = new List<int>();
        var collection2 = {collection};

        Assert.{method}(collection1, collection2);
        Assert.{method}(collection1, collection2, (int e1, int e2) => true);
        Assert.{method}(collection1, collection2, new MyComparer());
    }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSetAndComparer });
		}

		[Fact]
		public async void CastedSet_DoesNotTrigger()
		{
			var code = @"
using Xunit;
using System.Collections.Generic;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var expected = new HashSet<string> { ""bar"", ""foo"" };
        var actual = new HashSet<string> { ""foo"", ""bar"" };

         Assert.Equal(expected, actual);
         Assert.Equal(expected, (ISet<string>)actual);
         Assert.Equal((ISet<string>)expected, actual);
         Assert.Equal((ISet<string>)expected, (ISet<string>)actual);
    }
}";

			await Verify.VerifyAnalyzer(code);
		}

		public static MatrixTheoryData<string, (string type, string initializer)> MethodAndTypeAndInitializer =>
			new(
				new[] { "Equal", "NotEqual" },
				new[] {
					("HashSet<int>", "new HashSet<int>()"),
					("ImmutableHashSet<int>", "new HashSet<int>().ToImmutableHashSet()"),
					("MySet", "new MySet()")
				}
			);

		[Theory]
		[MemberData(nameof(MethodAndTypeAndInitializer), DisableDiscoveryEnumeration = true)]
		public async void SetWithLinearContainer_Triggers(
			string method,
			(string type, string initializer) collection)
		{
			var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = new List<int>();
        var collection2 = {collection.initializer};

        Assert.{method}(collection1, collection2);
        Assert.{method}(collection1, collection2, (int e1, int e2) => true);
        Assert.{method}(collection1, collection2, new MyComparer());

        Assert.{method}(collection2, collection1);
        Assert.{method}(collection2, collection1, (int e1, int e2) => true);
        Assert.{method}(collection2, collection1, new MyComparer());
    }}
}}";
			var expected = new[]
			{
				Verify
					.Diagnostic("xUnit2027")
					.WithSpan(13, 9, 13, 42 + method.Length)
					.WithArguments("List<int>", collection.type),
				Verify
					.Diagnostic("xUnit2027")
					.WithSpan(14, 9, 14, 68 + method.Length)
					.WithArguments("List<int>", collection.type),
				Verify
					.Diagnostic("xUnit2027")
					.WithSpan(15, 9, 15, 60 + method.Length)
					.WithArguments("List<int>", collection.type),

				Verify
					.Diagnostic("xUnit2027")
					.WithSpan(17, 9, 17, 42 + method.Length)
					.WithArguments(collection.type, "List<int>"),
				Verify
					.Diagnostic("xUnit2027")
					.WithSpan(18, 9, 18, 68 + method.Length)
					.WithArguments(collection.type, "List<int>"),
				Verify
					.Diagnostic("xUnit2027")
					.WithSpan(19, 9, 19, 60 + method.Length)
					.WithArguments(collection.type, "List<int>"),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, new[] { code, customSetAndComparer }, expected);
		}
	}
}
