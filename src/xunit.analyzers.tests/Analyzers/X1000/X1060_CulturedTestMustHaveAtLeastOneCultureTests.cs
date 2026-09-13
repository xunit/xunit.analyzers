using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CulturedTestMustHaveAtLeastOneCulture>;

public class X1060_CulturedTestMustHaveAtLeastOneCultureTests
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

				[[|CulturedFact(new string[] { })|]]
				[[|CulturedTheory(new string[] { })|]]
				public void Failure1() { }

				[[|CulturedFact(new string[0])|]]
				[[|CulturedTheory(new string[0])|]]
				public void Failure2() { }

				[[|CulturedFact([])|]]
				[[|CulturedTheory([])|]]
				public void Failure3() { }

				[[|CulturedFact(cultures: [])|]]
				[[|CulturedTheory(cultures: new string[] { })|]]
				public void Failure4() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp12, source);
	}
}
