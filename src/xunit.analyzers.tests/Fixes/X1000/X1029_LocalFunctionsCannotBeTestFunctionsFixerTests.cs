using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.LocalFunctionsCannotBeTestFunctions>;

public class X1029_LocalFunctionsCannotBeTestFunctionsFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
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

		await Verify.VerifyCodeFixFixAllNonAot(LanguageVersion.CSharp9, before, after, LocalFunctionsCannotBeTestFunctionsFixer.Key_RemoveAttribute);
	}

	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public void Method() {
					[[|CulturedFact(new[] { "en-US" })|]]
					void LocalFunction1() {
					}

					[[|CulturedTheory(new[] { "en-US" })|]]
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

		await Verify.VerifyCodeFixV3FixAllNonAot(LanguageVersion.CSharp9, before, after, LocalFunctionsCannotBeTestFunctionsFixer.Key_RemoveAttribute);
	}
}
