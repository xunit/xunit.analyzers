using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForNullCheck>;

public class AssertEqualShouldNotBeUsedForNullCheckTests
{
	public static TheoryData<string> Methods_All =
	[
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
		Constants.Asserts.StrictEqual,
		Constants.Asserts.NotStrictEqual,
		Constants.Asserts.Same,
		Constants.Asserts.NotSame,
	];
	public static TheoryData<string, string> Methods_Equal_WithReplacement = new()
	{
		{ Constants.Asserts.Equal, Constants.Asserts.Null },
		{ Constants.Asserts.NotEqual, Constants.Asserts.NotNull }
	};

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async Task ForFirstNullLiteral_StringOverload_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					string val = null;
					{{|#0:Xunit.Assert.{0}(null, val)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async Task ForFirstNullLiteral_StringOverload_WithCustomComparer_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					string val = null;
					{{|#0:Xunit.Assert.{0}(null, val, System.StringComparer.Ordinal)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.Equal, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.StrictEqual, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.Same, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.NotEqual, Constants.Asserts.NotNull)]
	[InlineData(Constants.Asserts.NotStrictEqual, Constants.Asserts.NotNull)]
	[InlineData(Constants.Asserts.NotSame, Constants.Asserts.NotNull)]
	public async Task ForFirstNullLiteral_ObjectOverload_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					object val = null;
					{{|#0:Xunit.Assert.{0}(null, val)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async Task ForFirstNullLiteral_ObjectOverload_WithCustomComparer_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					object val = null;
					{{|#0:Xunit.Assert.{0}(null, val, System.Collections.Generic.EqualityComparer<object>.Default)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.Equal, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.NotEqual, Constants.Asserts.NotNull)]
	[InlineData(Constants.Asserts.StrictEqual, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.NotStrictEqual, Constants.Asserts.NotNull)]
	public async Task ForFirstNullLiteral_GenericOverload_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					TestClass val = null;
					{{|#0:Xunit.Assert.{0}<TestClass>(null, val)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async Task ForFirstNullLiteral_GenericOverload_WithCustomComparer_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					TestClass val = null;
					{{|#0:Xunit.Assert.{0}<TestClass>(null, val, System.Collections.Generic.EqualityComparer<TestClass>.Default)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_All))]
	public async Task ForOtherLiteral_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					int val = 1;
					Xunit.Assert.{0}(1, val);
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods_All))]
	public async Task ForSecondNullLiteral_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					string val = null;
					Xunit.Assert.{0}(val, null);
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}
}
