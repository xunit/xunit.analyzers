using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SetsMustBeComparedWithEqualityComparer>;

public class SetsMustBeComparedWithEqualityComparerTests
{
	[Theory]
	[InlineData("Equal", ".ToImmutableHashSet()", 65)]
	[InlineData("NotEqual", ".ToImmutableHashSet()", 68)]
	public async void FindsWarning_ForSets(string method, string toImmutableCode, int endColumn)
	{
        var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = new HashSet<object>(){toImmutableCode};
        var collection2 = new HashSet<object>();

        Assert.{method}(collection1, collection2, (e1, e2) => true);
    }}
}}";

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(12, 9, 12, endColumn)
				.WithArguments(method);

		await Verify.VerifyAnalyzer(code, expected);
	}

	[Theory]
	[InlineData("Equal", "", 79)]
	[InlineData("NotEqual", "", 82)]
	[InlineData("Equal", ".ToImmutableHashSet()", 79)]
	[InlineData("NotEqual", ".ToImmutableHashSet()", 82)]
	public async void FindsWarning_ForSameTypeSetsButFuncWithTOverload(string method, string toImmutableCode, int endColumn)
	{
		var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = new HashSet<object>(){toImmutableCode};
        var collection2 = new HashSet<object>(){toImmutableCode};

        Assert.{method}(collection1, collection2, (object e1, object e2) => true);
    }}
}}";

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(12, 9, 12, endColumn)
				.WithArguments(method);

		await Verify.VerifyAnalyzer(code, expected);
	}

	[Theory]
	[InlineData("Equal", "(int e1, int e2) => true", 73)]
	[InlineData("Equal", "FuncComparer", 61)]
	[InlineData("Equal", "LocalFunc", 58)]
	[InlineData("Equal", "funcDelegate", 61)]
	[InlineData("NotEqual", "(int e1, int e2) => true", 76)]
	[InlineData("NotEqual", "FuncComparer", 64)]
	[InlineData("NotEqual", "LocalFunc", 61)]
	[InlineData("NotEqual", "funcDelegate", 64)]
	public async void FindsWarning_ForDifferentComparerFuncSyntax(string method, string comparerFuncSyntax, int endColumn)
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
        var collection1 = new HashSet<int>();
        var collection2 = new HashSet<int>();

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
				.Diagnostic()
				.WithSpan(26, 9, 26, endColumn)
				.WithArguments(method);

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, code, expected);
	}

	[Theory]
	[InlineData("Equal", "List", "List")]
	[InlineData("Equal", "HashSet", "List")]
	[InlineData("Equal", "List", "HashSet")]
	[InlineData("NotEqual", "List", "List")]
	[InlineData("NotEqual", "HashSet", "List")]
	[InlineData("NotEqual", "List", "HashSet")]
	public async void DoesNotFindWarning_ForNonSets(string method, string collection1Type, string collection2Type)
	{
		var code = @$"
using Xunit;
using System.Collections.Generic;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = new {collection1Type}<object>();
        var collection2 = new {collection2Type}<object>();

        Assert.{method}(collection1, collection2, (object e1, object e2) => true);
    }}
}}";

		await Verify.VerifyAnalyzer(code);
	}

	[Theory]
	[InlineData("Equal", "")]
	[InlineData("NotEqual", "")]
	[InlineData("Equal", ".ToImmutableHashSet()")]
	[InlineData("NotEqual", ".ToImmutableHashSet()")]
	public async void DoesNotFindWarning_ForSameTypeSetsWithIEnumerableTOverload(string method, string toImmutableCode)
	{
		var code = @$"
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection1 = new HashSet<object>(){toImmutableCode};
        var collection2 = new HashSet<object>(){toImmutableCode};

        Assert.{method}(collection1, collection2, (e1, e2) => true);
    }}
}}";

		await Verify.VerifyAnalyzer(code);
	}

	[Theory]
	[InlineData("Equal", "")]
	[InlineData("NotEqual", "")]
	[InlineData("Equal", ".ToImmutableHashSet()")]
	[InlineData("NotEqual", ".ToImmutableHashSet()")]
	public async void DoesNotFindWarning_ForSetsComparedWithEqualityComparer(string method, string toImmutableCode)
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
        var collection1 = new HashSet<int>(){toImmutableCode};
        var collection2 = new HashSet<int>(){toImmutableCode};

        Assert.{method}(collection1, collection2, new TestEqualityComparer());
    }}
}}";

		await Verify.VerifyAnalyzer(code);
	}
}
