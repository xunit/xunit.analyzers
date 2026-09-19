using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CulturedTestCultureValidation>;

public class X1070_CulturedTestCultureValidationTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				const string NullCulture = null;
				const string Culture = "en-US";

				[CulturedFact(new[] { "en-US" })]
				[CulturedTheory(new[] { "en-US", "" })]
				public void Success1() { }

				[CulturedFact(["en-US", "fr-FR"])]
				[CulturedTheory(cultures: [Culture])]
				public void Success2() { }

				[CulturedFact(new string[] { {|#0:null|} })]
				[CulturedTheory(new[] { "en-US", {|#1:null|} })]
				public void Failure1() { }

				[CulturedFact([{|#2:null|}, "en-US"])]
				[CulturedTheory(cultures: [{|#3:NullCulture|}])]
				public void Failure2() { }

				[CulturedFact(new string[] { {|#4:default|} })]
				[CulturedTheory(new string[] { "en-US", {|#5:default(string)|} })]
				public void Failure3() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1070").WithLocation(0),
			Verify.Diagnostic("xUnit1070").WithLocation(1),
			Verify.Diagnostic("xUnit1070").WithLocation(2),
			Verify.Diagnostic("xUnit1070").WithLocation(3),
			Verify.Diagnostic("xUnit1070").WithLocation(4),
			Verify.Diagnostic("xUnit1070").WithLocation(5),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp12, source, expected);
	}
}
