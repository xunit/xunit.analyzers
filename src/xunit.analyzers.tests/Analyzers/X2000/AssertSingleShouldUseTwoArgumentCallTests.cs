using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldUseTwoArgumentCall>;

public class AssertSingleShouldUseTwoArgumentCallTests
{
	public static TheoryData<string, string> GetEnumerables(
		string typeName,
		string comparison) =>
			AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksTests.GetEnumerables(typeName, comparison);

	[Theory]
	[MemberData(nameof(GetEnumerables), "int", "")]
	[MemberData(nameof(GetEnumerables), "string", "")]
	public async Task Containers_WithoutWhereClause_DoesNotTrigger(
		string collection,
		string _)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
					Xunit.Assert.Single({0});
			    }}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int", "f > 0")]
	[MemberData(nameof(GetEnumerables), "string", "f.Length > 0")]
	public async Task Containers_WithWhereClauseWithIndex_DoesNotTrigger(
		string collection,
		string comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
					Xunit.Assert.Single({0}.Where((f, i) => {1} && i > 0));
			    }}
			}}
			""", collection, comparison);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int", "f > 0")]
	[MemberData(nameof(GetEnumerables), "string", "f.Length > 0")]
	public async Task EnumurableEmptyCheck_WithChainedLinq_DoesNotTrigger(
		string collection,
		string comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
					Xunit.Assert.Single({0}.Where(f => {1}).Select(f => f));
			    }}
			}}
			""", collection, comparison);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(GetEnumerables), "int", "f > 0")]
	[MemberData(nameof(GetEnumerables), "string", "f.Length > 0")]
	public async Task Containers_WithWhereClause_Triggers(
		string collection,
		string comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
					[|Xunit.Assert.Single({0}.Where(f => {1}))|];
			    }}
			}}
			""", collection, comparison);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("")]
	[InlineData("123")]
	[InlineData(@"abc\n\t\\\""")]
	public async Task Strings_WithWhereClause_Triggers(string sampleString)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
					[|Xunit.Assert.Single("{0}".Where(f => f > 0))|];
			    }}
			}}
			""", sampleString);

		await Verify.VerifyAnalyzer(source);
	}
}
