using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X1003 = CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;
using Verify_X1006 = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

public class ConvertToFactFixTests
{
	[Fact]
	public async Task FixAll_From_X1003()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void [|TestMethod1|](int a) { }

				[Theory]
				public void [|TestMethod2|](string b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod1(int a) { }

				[Fact]
				public void TestMethod2(string b) { }
			}
			""";

		await Verify_X1003.VerifyCodeFixFixAll(before, after, ConvertToFactFix.Key_ConvertToFact);
	}

	[Fact]
	public async Task FixAll_From_X1006()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void [|TestMethod1|]() { }

				[Theory]
				public void [|TestMethod2|]() { }
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

		await Verify_X1006.VerifyCodeFixFixAll(before, after, ConvertToFactFix.Key_ConvertToFact);
	}
}
