using System;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.UseAssertFailInsteadOfBooleanAssert>;
using Verify_Unsupported = CSharpVerifier<UseAssertFailInsteadOfBooleanAssertTests.Analyzer_Pre25>;

public class UseAssertFailInsteadOfBooleanAssertTests
{
	const string codeTemplate = @"
public class TestClass {{
    [Xunit.Fact]
    public void TestMethod() {{
        Xunit.Assert.{0}({1}, ""failure message"");
    }}
}}";

	[Theory]
	[InlineData(Constants.Asserts.True, "false")]
	[InlineData(Constants.Asserts.False, "true")]
	public async void SignalsFor25(
		string assertion,
		string targetValue)
	{
		var source = string.Format(codeTemplate, assertion, targetValue);
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 52)
				.WithArguments(assertion, targetValue);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "true")]
	[InlineData(Constants.Asserts.False, "false")]
	public async void DoesNotSignalForNonFailure(
		string assertion,
		string targetValue)
	{
		var source = string.Format(codeTemplate, assertion, targetValue);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotSignalForNonConstantInvocation()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() {
        var value = (1 != 2);
        Xunit.Assert.False(value, ""failure message"");
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "false")]
	[InlineData(Constants.Asserts.False, "true")]
	public async void DoNotSignalForPre25(
		string assertion,
		string targetValue)
	{
		var source = string.Format(codeTemplate, assertion, targetValue);

		await Verify_Unsupported.VerifyAnalyzer(source);
	}

	internal class Analyzer_Pre25 : UseAssertFailInsteadOfBooleanAssert
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Assert(compilation, new Version(2, 4, 999));
	}
}
