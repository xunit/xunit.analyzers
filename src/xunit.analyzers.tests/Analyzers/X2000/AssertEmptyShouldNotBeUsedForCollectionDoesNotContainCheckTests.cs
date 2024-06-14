using System;
using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck>;



public class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckTests
{
	public static TheoryData<string> GetEnumerables(string typeName)
	{
		return new TheoryData<string>()
		{
			$"new System.Collections.Generic.List<{typeName}>()",
			$"new System.Collections.Generic.HashSet<{typeName}>()",
			$"new System.Collections.ObjectModel.Collection<{typeName}>()",
			$"new {typeName}[0]",
			$"System.Linq.Enumerable.Empty<{typeName}>()"
		};
	}

	public static TheoryData<string> GetSampleStrings()
	{
		return new TheoryData<string>()
		{
			String.Empty,
			"123",
			@"abc\n\t\\\"""
		};
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int")]
	public async Task FindsWarningForIntEnumerableDoesNotContainCheckWithEmpty(string collection)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        [|Xunit.Assert.Empty({collection}.Where(f => f > 0))|];
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "string")]
	public async Task FindsWarningForStringEnumerableDoesNotContainCheckWithEmpty(string collection)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        [|Xunit.Assert.Empty({collection}.Where(f => f.Length > 0))|];
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetSampleStrings))]
	public async Task FindsWarningForStringDoesNotContainCheckWithEmpty(string sampleString)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        [|Xunit.Assert.Empty(""{sampleString}"".Where(f => f > 0))|];
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int")]
	public async Task DoesNotFindWarningForIntEnumerableDoesNotContainCheckWithEmptyWithIndex(string collection)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        Xunit.Assert.Empty({collection}.Where((f, i) => f > 0 && i > 0));
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "string")]
	public async Task DoesNotFindWarningForStringEnumerableDoesNotContainCheckWithEmptyWithIndex(string collection)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        Xunit.Assert.Empty({collection}.Where((f, i) => f.Length > 0 && i > 0));
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int")]
	[MemberData(nameof(GetEnumerables), "string")]
	public async Task DoesNotFindWarningForEnumerableEmptyCheckWithoutLinq(string collection)
	{
		var source = $@"
class TestClass
{{
    void TestMethod()
    {{
        Xunit.Assert.Empty({collection});
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

    [Theory]
    [MemberData(nameof(GetEnumerables), "int")]
    [MemberData(nameof(GetEnumerables), "string")]
    public async Task DoesNotFindWarningForEnumurableEmptyCheckWithChainedLinq(string collection)
    {
        var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        Xunit.Assert.Empty({collection}.Where(f => f.ToString().Length > 0).Select(f => f));
    }}
}}";
        await Verify.VerifyAnalyzer(source);
    }
}
