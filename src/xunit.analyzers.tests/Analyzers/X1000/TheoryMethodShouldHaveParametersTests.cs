using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

public class TheoryMethodShouldHaveParametersTests
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

	[Fact]
	public async Task TheoryMethodWithParameters_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
				[Xunit.Theory]
				public void TestMethod(string s) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryMethodWithoutParameters_Triggers()
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
