using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

public class AssertEqualShouldNotBeUsedForBoolLiteralCheckTests
{
	public static TheoryData<string> Methods = new()
	{
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
		Constants.Asserts.StrictEqual,
		Constants.Asserts.NotStrictEqual,
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarning_ForFirstBoolLiteral(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        bool val = true;
        Xunit.Assert.{method}(true, val);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 33 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.Equal)]
	[InlineData(Constants.Asserts.NotEqual)]
	public async void FindsWarning_ForFirstBoolLiteral_WithCustomComparer(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        bool val = false;
        Xunit.Assert.{method}(false, val, System.Collections.Generic.EqualityComparer<bool>.Default);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 93 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForFirstBoolLiteral_ObjectOverload(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object val = false;
        Xunit.Assert.{method}(true, val);
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForOtherLiteral(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        int val = 1;
        Xunit.Assert.{method}(1, val);
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForSecondBoolLiteral(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        bool val = false;
        Xunit.Assert.{method}(val, true);
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}
}
