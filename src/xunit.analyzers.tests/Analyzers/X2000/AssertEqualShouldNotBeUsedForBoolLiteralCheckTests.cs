using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

public class AssertEqualShouldNotBeUsedForBoolLiteralCheckTests
{
	public static TheoryData<string> Methods =
	[
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
		Constants.Asserts.StrictEqual,
		Constants.Asserts.NotStrictEqual,
	];

	[Theory]
	[InlineData(Constants.Asserts.Equal, Constants.Asserts.True)]
	[InlineData(Constants.Asserts.StrictEqual, Constants.Asserts.True)]
	[InlineData(Constants.Asserts.NotEqual, Constants.Asserts.False)]
	[InlineData(Constants.Asserts.NotStrictEqual, Constants.Asserts.False)]
	public async Task ForFirstBoolLiteral_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        bool val = true;
			        {{|#0:Xunit.Assert.{0}(true, val)|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.Equal, Constants.Asserts.False)]
	[InlineData(Constants.Asserts.NotEqual, Constants.Asserts.True)]
	public async Task ForFirstBoolLiteral_WithCustomComparer_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        bool val = false;
			        {{|#0:Xunit.Assert.{0}(false, val, System.Collections.Generic.EqualityComparer<bool>.Default)|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForFirstBoolLiteral_ObjectOverload_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        object val = false;
			        Xunit.Assert.{0}(true, val);
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
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
	[MemberData(nameof(Methods))]
	public async Task ForSecondBoolLiteral_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        bool val = false;
			        Xunit.Assert.{0}(val, true);
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}
}
