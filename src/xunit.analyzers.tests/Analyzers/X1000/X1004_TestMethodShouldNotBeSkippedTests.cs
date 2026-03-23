using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodShouldNotBeSkipped>;

public class X1004_TestMethodShouldNotBeSkippedTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void Fact_NotSkippedTest_DoesNotTrigger() { }

				[Theory]
				public void Theory_NotSkippedTest_DoesNotTrigger() { }

				[Fact([|Skip="Lazy"|])]
				public void Fact_SkippedTest_Triggers() { }

				[Theory([|Skip="Lazy"|])]
				public void Theory_SkippedTest_Triggers() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static bool Condition { get; set; }

				[CulturedFact(new[] { "en-US" })]
				public void Fact_NotSkippedTest_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				public void Theory_NotSkippedTest_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" }, [|Skip="Lazy"|])]
				public void Fact_SkippedTest_Triggers() { }

				[CulturedTheory(new[] { "en-US" }, [|Skip="Lazy"|])]
				public void Theory_SkippedTest_Triggers() { }

				// Skip + SkipUnless does not trigger

				[Fact(Skip = "Lazy", SkipUnless = nameof(Condition))]
				public void Fact_SkippedTestWithSkipUnless_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" }, Skip = "Lazy", SkipUnless = nameof(Condition))]
				public void CulturedFact_SkippedTestWithSkipUnless_DoesNotTrigger() { }

				[Theory(Skip = "Lazy", SkipUnless = nameof(Condition))]
				public void TheorySkippedTestWithSkipUnless_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" }, Skip = "Lazy", SkipUnless = nameof(Condition))]
				public void CulturedTheorySkippedTestWithSkipUnless_DoesNotTrigger() { }

				// Skip + SkipWhen does not trigger

				[Fact(Skip = "Lazy", SkipWhen = nameof(Condition))]
				public void Fact_SkippedTestWithSkipWhen_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" }, Skip = "Lazy", SkipWhen = nameof(Condition))]
				public void CulturedFact_SkippedTestWithSkipWhen_DoesNotTrigger() { }

				[Theory(Skip = "Lazy", SkipWhen = nameof(Condition))]
				public void TheorySkippedTestWithSkipWhen_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" }, Skip = "Lazy", SkipWhen = nameof(Condition))]
				public void CulturedTheorySkippedTestWithSkipWhen_DoesNotTrigger() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
