using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;

public class FactMethodMustNotHaveParametersTests
{
	[Fact]
	public async Task FactWithNoParameters_DoesNotTrigger()
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
	public async Task TheoryWithParameters_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
			    [Xunit.Theory]
			    public void TestMethod(string p) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task FactWithParameters_Triggers()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
			    [Xunit.Fact]
			    public void [|TestMethod|](string p) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
