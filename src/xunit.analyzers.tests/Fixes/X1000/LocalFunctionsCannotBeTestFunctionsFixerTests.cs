using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.LocalFunctionsCannotBeTestFunctions>;

public class LocalFunctionsCannotBeTestFunctionsFixerTests
{
	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async Task LocalFunctionsCannotHaveTestAttributes(string attribute)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;

			public class TestClass {{
				public void Method() {{
					[[|{0}|]]
					void LocalFunction() {{
					}}
				}}
			}}
			""", attribute);
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public void Method() {
					void LocalFunction() {
					}
				}
			}
			""";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp9, before, after, LocalFunctionsCannotBeTestFunctionsFixer.Key_RemoveAttribute);
	}
}
