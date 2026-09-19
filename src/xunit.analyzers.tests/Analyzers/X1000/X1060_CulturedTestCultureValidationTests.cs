using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CulturedTestCultureValidation>;

public class X1060_CulturedTestCultureValidationTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedFact(new[] { "en-US" })]
				[CulturedTheory(new[] { "en-US" })]
				public void Success1() { }

				[CulturedFact(["en-US"])]
				[CulturedTheory(["en-US"])]
				public void Success2() { }

				[CulturedFact(new string[] { "en-US", "fr-FR" })]
				[CulturedTheory(cultures: ["en-US"])]
				public void Success3() { }

				[{|xUnit1060:CulturedFact(new string[] { })|}]
				[{|xUnit1060:CulturedTheory(new string[] { })|}]
				public void Failure1() { }

				[{|xUnit1060:CulturedFact(new string[0])|}]
				[{|xUnit1060:CulturedTheory(new string[0])|}]
				public void Failure2() { }

				[{|xUnit1060:CulturedFact([])|}]
				[{|xUnit1060:CulturedTheory([])|}]
				public void Failure3() { }

				[{|xUnit1060:CulturedFact(cultures: [])|}]
				[{|xUnit1060:CulturedTheory(cultures: new string[] { })|}]
				public void Failure4() { }

				[{|xUnit1060:CulturedFact(null)|}]
				[{|xUnit1060:CulturedTheory(cultures: null)|}]
				public void Failure5() { }

				[{|xUnit1060:CulturedFact(default)|}]
				[{|xUnit1060:CulturedTheory((string[])null)|}]
				public void Failure6() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp12, source);
	}
}
