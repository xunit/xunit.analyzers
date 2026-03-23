using System.Threading.Tasks;
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
}
