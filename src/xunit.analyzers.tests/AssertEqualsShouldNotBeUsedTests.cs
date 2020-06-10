using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertEqualsShouldNotBeUsed>;

namespace Xunit.Analyzers
{
	public class AssertEqualsShouldNotBeUsedTests
	{
		[Theory]
		[InlineData("Equals", "Equal")]
		[InlineData("ReferenceEquals", "Same")]
		public async void FindsHiddenDiagnosticWhenProhibitedMethodIsUsed(string method, string replacement)
		{
			var source =
@"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(null, null);
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 30 + method.Length).WithMessage($"'Assert.{method}(object, object)' is obsolete: 'This is an override of Object.{method}(). Call Assert.{replacement}() instead.'"),
				Verify.Diagnostic().WithSpan(2, 5, 2, 30 + method.Length).WithSeverity(DiagnosticSeverity.Hidden).WithArguments($"Assert.{method}()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}
	}
}
