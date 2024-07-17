using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X1001 = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;
using Verify_X1005 = CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

public class ConvertToTheoryFixTests
{
	[Fact]
	public async Task From_X1001()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void [|TestMethod|](int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Theory]
			    public void TestMethod(int a) { }
			}
			""";

		await Verify_X1001.VerifyCodeFix(before, after, ConvertToTheoryFix.Key_ConvertToTheory);
	}

	[Fact]
	public async Task From_X1005()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    [InlineData(42)]
			    public void [|TestMethod|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Theory]
			    [InlineData(42)]
			    public void TestMethod() { }
			}
			""";

		await Verify_X1005.VerifyCodeFix(before, after, ConvertToTheoryFix.Key_ConvertToTheory);
	}
}
