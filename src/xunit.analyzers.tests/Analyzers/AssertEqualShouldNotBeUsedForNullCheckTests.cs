using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForNullCheck>;

public class AssertEqualShouldNotBeUsedForNullCheckTests
{
	public static TheoryData<string> Methods_All = new()
	{
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
		Constants.Asserts.StrictEqual,
		Constants.Asserts.NotStrictEqual,
		Constants.Asserts.Same,
		Constants.Asserts.NotSame,
	};
	public static TheoryData<string> Methods_Equal = new()
	{
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
	};

	[Theory]
	[MemberData(nameof(Methods_Equal))]
	public async void FindsWarning_ForFirstNullLiteral_StringOverload(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(null, val);
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
	[MemberData(nameof(Methods_Equal))]
	public async void FindsWarning_ForFirstNullLiteral_StringOverload_WithCustomComparer(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(null, val, System.StringComparer.Ordinal);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 64 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_All))]
	public async void FindsWarning_ForFirstNullLiteral_ObjectOverload(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object val = null;
        Xunit.Assert.{method}(null, val);
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
	[MemberData(nameof(Methods_Equal))]
	public async void FindsWarning_ForFirstNullLiteral_ObjectOverload_WithCustomComparer(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object val = null;
        Xunit.Assert.{method}(null, val, System.Collections.Generic.EqualityComparer<object>.Default);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 94 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.Equal)]
	[InlineData(Constants.Asserts.NotEqual)]
	[InlineData(Constants.Asserts.StrictEqual)]
	[InlineData(Constants.Asserts.NotStrictEqual)]
	public async void FindsWarning_ForFirstNullLiteral_GenericOverload(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        TestClass val = null;
        Xunit.Assert.{method}<TestClass>(null, val);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 44 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_Equal))]
	public async void FindsWarning_ForFirstNullLiteral_GenericOverload_WithCustomComparer(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        TestClass val = null;
        Xunit.Assert.{method}<TestClass>(null, val, System.Collections.Generic.EqualityComparer<TestClass>.Default);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 108 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_All))]
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
	[MemberData(nameof(Methods_All))]
	public async void DoesNotFindWarning_ForSecondNullLiteral(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(val, null);
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}
}
