using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class X1008_DataAttributeShouldBeUsedOnATheoryTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void Fact_NoData_DoesNotTrigger() { }

				[Fact]
				[InlineData]
				public void Fact_InlineData_DoesNotTrigger() { }

				[Fact]
				[MemberData("")]
				public void Fact_MemberData_DoesNotTrigger() { }

				[Fact]
				[ClassData(typeof(string))]
				public void Fact_ClassData_DoesNotTrigger() { }

				[Theory]
				[InlineData]
				public void Theory_InlineData_DoesNotTrigger() { }

				[Theory]
				[MemberData("")]
				public void Theory_MemberData_DoesNotTrigger() { }

				[Theory]
				[ClassData(typeof(string))]
				public void Theory_ClassData_DoesNotTrigger() { }

				[InlineData]
				public void [|NonFact_InlineData_Triggers|]() { }

				[MemberData("")]
				public void [|NonFact_MemberData_Triggers|]() { }

				[ClassData(typeof(string))]
				public void [|NonFact_ClassData_Triggers|]() { }
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
				[CulturedFact(new[] { "en-US" })]
				[InlineData]
				public void CulturedFact_InlineData_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" })]
				[MemberData("")]
				public void CulturedFact_MemberData_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void CulturedFact_ClassData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				public void CulturedTheory_InlineData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[MemberData("")]
				public void CulturedTheory_MemberData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void CulturedTheory_ClassData_DoesNotTrigger() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
