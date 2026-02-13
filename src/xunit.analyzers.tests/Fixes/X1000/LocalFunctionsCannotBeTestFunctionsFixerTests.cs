using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.LocalFunctionsCannotBeTestFunctions>;

public class LocalFunctionsCannotBeTestFunctionsFixerTests
{
	[Fact]
	public async Task FixAll_RemovesAttributesFromAllLocalFunctions()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public void Method() {
					[[|Fact|]]
					void LocalFunction1() {
					}

					[[|Theory|]]
					void LocalFunction2() {
					}
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public void Method() {
					void LocalFunction1() {
					}

					void LocalFunction2() {
					}
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(LanguageVersion.CSharp9, before, after, LocalFunctionsCannotBeTestFunctionsFixer.Key_RemoveAttribute);
	}
}
