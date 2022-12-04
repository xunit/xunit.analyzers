using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;

public class AssertIsTypeShouldNotBeUsedForAbstractTypeTests
{
	public static TheoryData<string> Methods = new()
	{
		"IsType",
		"IsNotType",
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsError_Interface(string method)
	{
		var source = $@"
using System;
using Xunit;

class TestClass {{
    void TestMethod() {{
        Assert.{method}<IDisposable>(new object());
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(7, 9, 7, 43 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("interface", "System.IDisposable");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsError_AbstractClass(string method)
	{
		var source = $@"
using System.IO;
using Xunit;

class TestClass {{
    void TestMethod() {{
        Assert.{method}<Stream>(new object());
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(7, 9, 7, 38 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("abstract class", "System.IO.Stream");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsError_UsingStatic(string method)
	{
		var source = $@"
using System;
using static Xunit.Assert;

class TestClass {{
    void TestMethod() {{
        {method}<IDisposable>(new object());
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(7, 9, 7, 36 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("interface", "System.IDisposable");

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindError_NonAbstractClass(string method)
	{
		var source = $@"
using Xunit;

class TestClass {{
    void TestMethod() {{
        Assert.{method}<string>(new object());
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
