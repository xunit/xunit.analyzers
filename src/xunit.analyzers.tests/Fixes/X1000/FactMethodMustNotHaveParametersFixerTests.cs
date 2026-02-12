using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;

public class FactMethodMustNotHaveParametersFixerTests
{
	[Fact]
	public async Task FixAll_RemovesParametersFromAllMethods()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void [|TestMethod1|](int x) { }

				[Fact]
				public void [|TestMethod2|](string a, int b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod1() { }

				[Fact]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, FactMethodMustNotHaveParametersFixer.Key_RemoveParameters);
	}
}
