using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

public class TheoryMethodMustHaveTestDataTests
{
	[Fact]
	public async Task FactMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryMethodWithDataAttributes_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData]
				public void TestMethod1() { }

				[Theory]
				[MemberData("")]
				public void TestMethod2() { }

				[Theory]
				[ClassData(typeof(string))]
				public void TestMethod3() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryMethodMissingData_Triggers()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				public void [|TestMethod|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
