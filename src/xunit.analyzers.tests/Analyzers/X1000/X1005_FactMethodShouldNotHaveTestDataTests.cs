using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

public class X1005_FactMethodShouldNotHaveTestDataTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void FactWithNoDataAttributes_DoesNotTrigger() { }

				[Fact]
				[InlineData]
				public void [|FactWithInlineData_Triggers|]() { }

				[Fact]
				[MemberData("")]
				public void [|FactWithMemberData_Triggers|]() { }

				[Fact]
				[ClassData(typeof(string))]
				public void [|FactWithClassData_Triggers|]() { }

				[Theory]
				[InlineData]
				public void TheoryWithInlineData_DoesNotTrigger() { }

				[Theory]
				[MemberData("")]
				public void TheoryWithMemberData_DoesNotTrigger() { }

				[Theory]
				[ClassData(typeof(string))]
				public void TheoryWithClassData_DoesNotTrigger() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source1 = /* lang=c#-test */ "public class DerivedFactAttribute: Xunit.FactAttribute {}";
		var source2 = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[DerivedFactAttribute]
				[InlineData]
				public void TestMethod1() { }

				[DerivedFactAttribute]
				[MemberData("")]
				public void TestMethod2() { }

				[DerivedFactAttribute]
				[ClassData(typeof(string))]
				public void TestMethod3() { }
			}
			""";

		await Verify.VerifyAnalyzerNonAot([source1, source2]);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				[ClassData(typeof(string))]
				public void [|FactWithClassData_Triggers|]() { }

				[Theory]
				[ClassData<string>]
				public void TheoryWithClassData_Generic_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" })]
				public void CulturedFactWithNoDataAttributes_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" })]
				[InlineData]
				public void [|CulturedFactWithInlineData_Triggers|]() { }

				[CulturedFact(new[] { "en-US" })]
				[MemberData("")]
				public void [|CulturedFactWithMemberData_Triggers|]() { }

				[CulturedFact(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void [|CulturedFactWithClassData_Triggers|]() { }

				[CulturedFact(new[] { "en-US" })]
				[ClassData<string>]
				public void [|CulturedFactWithClassData_Generic_Triggers|]() { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				public void CulturedTheoryWithInlineData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[MemberData("")]
				public void CulturedTheoryWithMemberData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void CulturedTheoryWithClassData_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				[ClassData<string>]
				public void CulturedTheoryWithClassData_Generic_DoesNotTrigger() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source);
	}
}
