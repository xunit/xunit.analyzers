using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck>;

public class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckTests
{
	public static TheoryData<string, string> GetEnumerables(
		string typeName,
		string comparison) =>
			new()
			{
				{ $"new System.Collections.Generic.List<{typeName}>()", comparison },
				{ $"new System.Collections.Generic.HashSet<{typeName}>()", comparison },
				{ $"new System.Collections.ObjectModel.Collection<{typeName}>()", comparison },
				{ $"new {typeName}[0]", comparison },
				{ $"System.Linq.Enumerable.Empty<{typeName}>()", comparison },
			};

	[Theory]
	[MemberData(nameof(GetEnumerables), "int", "")]
	[MemberData(nameof(GetEnumerables), "string", "")]
	public async Task Containers_WithoutWhereClause_DoesNotTrigger(
		string collection,
		string _)
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
	[MemberData(nameof(GetEnumerables), "int", "f > 0")]
	[MemberData(nameof(GetEnumerables), "string", "f.Length > 0")]
	public async Task Containers_WithWhereClause_Triggers(
		string collection,
		string comparison)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        [|Xunit.Assert.Empty({collection}.Where(f => {comparison}))|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int", "f > 0")]
	[MemberData(nameof(GetEnumerables), "string", "f.Length > 0")]
	public async Task Containers_WithWhereClauseWithIndex_DoesNotTrigger(
		string collection,
		string comparison)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        Xunit.Assert.Empty({collection}.Where((f, i) => {comparison} && i > 0));
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int", "f > 0")]
	[MemberData(nameof(GetEnumerables), "string", "f.Length > 0")]
	public async Task DoesNotFindWarningForEnumurableEmptyCheckWithChainedLinq(
		string collection,
		string comparison)
	{
		var source = $@"
using System.Linq;
class TestClass
{{
    void TestMethod()
    {{
        Xunit.Assert.Empty({collection}.Where(f => {comparison}).Select(f => f));
    }}
}}";
		await Verify.VerifyAnalyzer(source);
	}

	public static TheoryData<string> GetSampleStrings() =>
		new(string.Empty, "123", @"abc\n\t\\\""");

	[Theory]
	[MemberData(nameof(GetSampleStrings))]
	public async Task Strings_WithWhereClause_DoesNotTrigger(string sampleString)
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
}
