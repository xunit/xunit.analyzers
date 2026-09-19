using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CulturedTestCultureValidation>;

public class X1071_CulturedTestCultureValidationTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				const string Culture = "en-US";

				[CulturedFact(new[] { "en-US", "fr-FR", "" })]
				[CulturedTheory(["en-US", "en-GB"])]
				public void Success() { }

				[CulturedFact(new[] { "en-US", {|#0:"en-US"|} })]
				[CulturedTheory(cultures: ["en-US", "fr-FR", {|#1:"en-US"|}, {|#2:"fr-FR"|}])]
				public void Failure1() { }

				[CulturedFact(["en-US", {|#3:"en-us"|}, {|#4:Culture|}])]
				[CulturedTheory(new string[] { "", {|#5:""|} })]
				public void Failure2() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1071").WithLocation(0).WithArguments("en-US"),
			Verify.Diagnostic("xUnit1071").WithLocation(1).WithArguments("en-US"),
			Verify.Diagnostic("xUnit1071").WithLocation(2).WithArguments("fr-FR"),
			Verify.Diagnostic("xUnit1071").WithLocation(3).WithArguments("en-us"),
			Verify.Diagnostic("xUnit1071").WithLocation(4).WithArguments("en-US"),
			Verify.Diagnostic("xUnit1071").WithLocation(5).WithArguments(""),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp12, source, expected);
	}
}
