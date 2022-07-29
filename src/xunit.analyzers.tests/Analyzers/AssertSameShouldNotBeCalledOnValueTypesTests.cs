﻿using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class AssertSameShouldNotBeCalledOnValueTypesTests
{
	public static TheoryData<string, string> Methods_WithReplacement = new()
	{
		{ Constants.Asserts.Same, Constants.Asserts.Equal },
		{ Constants.Asserts.NotSame, Constants.Asserts.NotEqual },
	};

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async void FindsWarningForTwoValueParameters(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        int a = 0;
        Xunit.Assert.{method}(0, a);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 28 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async void FindsWarningForFirstValueParameters(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object a = 0;
        Xunit.Assert.{method}(0, a);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 28 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async void FindsWarningForSecondValueParameters(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object a = 0;
        Xunit.Assert.{method}(a, 0);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 28 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	// https://github.com/xunit/xunit/issues/2395
	public async void DoesNotFindWarningForUserDefinedImplicitConversion(
		string method,
		string replacement)
	{
		_ = replacement; // Verifies that diagnostic is not issued, so the replacement method is not needed

		var source = $@"
public class TestClass
{{
    public void TestMethod()
    {{
		var o = new object();

        Xunit.Assert.{method}((MyBuggyInt)42, o);
        Xunit.Assert.{method}((MyBuggyInt)(int?)42, o);
        Xunit.Assert.{method}((MyBuggyIntBase)42, o);
        Xunit.Assert.{method}((MyBuggyIntBase)(int?)42, o);

        Xunit.Assert.{method}(o, (MyBuggyInt)42);
        Xunit.Assert.{method}(o, (MyBuggyInt)(int?)42);
        Xunit.Assert.{method}(o, (MyBuggyIntBase)42);
        Xunit.Assert.{method}(o, (MyBuggyIntBase)(int?)42);
    }}
}}

public abstract class MyBuggyIntBase
{{
    public static implicit operator MyBuggyIntBase(int i) => new MyBuggyInt();
}}

public class MyBuggyInt : MyBuggyIntBase
{{
    public MyBuggyInt()
	{{
	}}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
