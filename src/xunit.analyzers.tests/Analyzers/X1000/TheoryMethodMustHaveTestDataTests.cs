using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

public class TheoryMethodMustHaveTestDataTests
{
	[Fact]
	public async Task FactMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
				[Xunit.Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("InlineData")]
	[InlineData("MemberData(\"\")")]
	[InlineData("ClassData(typeof(string))")]
	public async Task TheoryMethodWithDataAttributes_DoesNotTrigger(string dataAttribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
				[Xunit.Theory]
				[Xunit.{0}]
				public void TestMethod() {{ }}
			}}
			""", dataAttribute);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryMethodMissingData_Triggers()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				[Xunit.Theory]
				public void [|TestMethod|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
