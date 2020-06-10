using Microsoft.CodeAnalysis.Testing;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

namespace Xunit.Analyzers
{
	public class AssertEqualLiteralValueShouldBeFirstFixerTests
	{
		static readonly string Template = @"
public class TestClass
{{
    [Xunit.Fact]
    public void TestMethod()
    {{
        var i = 0;
        Xunit.{0};
    }}
}}";

		[Fact]
		public async void SwapArguments()
		{
			var source = string.Format(Template, "Assert.Equal(i, 0)");
			var fixedSource = string.Format(Template, "Assert.Equal(0, i)");

			var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
			await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
		}

		[Fact]
		public async void NamedArgumentsOnlySwapsArgumentValues()
		{
			var source = string.Format(Template, "Assert.Equal(actual: 0, expected: i)");
			var fixedSource = string.Format(Template, "Assert.Equal(actual: i, expected: 0)");

			var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
			await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
		}

		[Fact]
		public async void NamedArgumentsInCorrectPositionOnlySwapsArgumentValues()
		{
			var source = string.Format(Template, "Assert.Equal(expected: i, actual: 0)");
			var fixedSource = string.Format(Template, "Assert.Equal(expected: 0, actual: i)");

			var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
			await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
		}

		[Fact]
		public async void NamedArgumentsTakePossibleThirdParameterIntoAccount()
		{
			var source = string.Format(Template, "Assert.Equal(comparer: null, actual: 0, expected: i)");
			var fixedSource = string.Format(Template, "Assert.Equal(comparer: null, actual: i, expected: 0)");

			var expected = Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual, comparer)", "TestMethod", "TestClass");
			await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
		}

		[Fact]
		public async void PartiallyNamedArgumentsInCorrectPositionOnlySwapsArgumentValues()
		{
			// C# 7.2 supports this new supported "non-trailing named arguments"

			var source = string.Format(Template, "Assert.Equal(expected: i, 0)");
			var fixedSource = string.Format(Template, "Assert.Equal(expected: 0, i)");

			DiagnosticResult[] expected =
			{
				Verify.Diagnostic().WithLocation(8, 9).WithArguments("0", "Assert.Equal(expected, actual)", "TestMethod", "TestClass"),
				Verify.CompilerError("CS1738").WithLocation(8, 41).WithMessage("Named argument specifications must appear after all fixed arguments have been specified"),
			};
			await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
		}
	}
}
