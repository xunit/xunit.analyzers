using Microsoft.CodeAnalysis;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

namespace Xunit.Analyzers
{
	public class AssertSameShouldNotBeCalledOnValueTypesTests
	{
		[Theory]
		[InlineData("Same")]
		[InlineData("NotSame")]
		public async void FindsWarningForTwoValueParameters(string method)
		{
			var source =
@"class TestClass { void TestMethod() {
    int a = 0;
    Xunit.Assert." + method + @"(0, a);
} }";

			var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 24 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()", "int");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[InlineData("Same")]
		[InlineData("NotSame")]
		public async void FindsWarningForFirstValueParameters(string method)
		{
			var source =
@"class TestClass { void TestMethod() {
    object a = 0;
    Xunit.Assert." + method + @"(0, a);
} }";

			var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 24 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()", "int");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[InlineData("Same")]
		[InlineData("NotSame")]
		public async void FindsWarningForSecondValueParameters(string method)
		{
			var source =
@"class TestClass { void TestMethod() {
    object a = 0;
    Xunit.Assert." + method + @"(a, 0);
} }";

			var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 24 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()", "int");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}
	}
}
