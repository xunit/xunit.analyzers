using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.LocalFunctionsCannotBeTestFunctions>;

public class LocalFunctionsCannotBeTestFunctionsFixerTests
{
	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async void LocalFunctionsCannotHaveTestAttributes(string attribute)
	{
		var before = $@"
using Xunit;

public class TestClass {{
    public void Method() {{
        [[|{attribute}|]]
        void LocalFunction() {{
        }}
    }}
}}";
		var after = @"
using Xunit;

public class TestClass {
    public void Method() {
        void LocalFunction() {
        }
    }
}";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp9, before, after, LocalFunctionsCannotBeTestFunctionsFixer.Key_RemoveAttribute);
	}
}
