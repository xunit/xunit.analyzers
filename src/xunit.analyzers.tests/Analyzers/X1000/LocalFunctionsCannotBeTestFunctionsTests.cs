using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.LocalFunctionsCannotBeTestFunctions>;

public class LocalFunctionsCannotBeTestFunctionsTests
{
	[Fact]
	public async void DoesNotTriggerOnLocalFunctionWithoutAttributes()
	{
		var source = @"
using Xunit;

public class TestClass {
    public void Method() {
        void LocalFunction() {
        }
    }
}";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
	}

	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	[InlineData("InlineData(42)")]
	[InlineData("MemberData(nameof(MyData))")]
	[InlineData("ClassData(typeof(TestClass))")]
	public async void LocalFunctionsCannotHaveTestAttributes(string attribute)
	{
		var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public void Method() {{
        [{attribute}]
        void LocalFunction() {{
        }}
    }}

    public static IEnumerable<object[]> MyData;
}}";

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(7, 10, 7, 10 + attribute.Length)
				.WithArguments($"[{attribute}]");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source, expected);
	}
}
