using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X1003 = CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;
using Verify_X1006 = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

public class ConvertToFactFixTests
{
	[Fact]
	public async Task From_X1003()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void [|TestMethod|](int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod(int a) { }
			}
			""";

		await Verify_X1003.VerifyCodeFix(before, after, ConvertToFactFix.Key_ConvertToFact);
	}

	[Fact]
	public async Task From_X1006()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void [|TestMethod|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify_X1006.VerifyCodeFix(before, after, ConvertToFactFix.Key_ConvertToFact);
	}
}
