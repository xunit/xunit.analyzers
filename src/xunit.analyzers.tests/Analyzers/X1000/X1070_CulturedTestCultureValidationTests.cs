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

				[CulturedFact(new string[] { {|xUnit1070:null|} })]
				[CulturedTheory(new[] { "en-US", {|xUnit1070:null|} })]
				public void Failure1() { }

				[CulturedFact([{|xUnit1070:null|}, "en-US"])]
				[CulturedTheory(cultures: [{|xUnit1070:NullCulture|}])]
				public void Failure2() { }

				[CulturedFact(new string[] { {|xUnit1070:default|} })]
				[CulturedTheory(new string[] { "en-US", {|xUnit1070:default(string)|} })]
				public void Failure3() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp12, source);
	}
}
