using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X1001 = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;
using Verify_X1005 = CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

public class ConvertToTheoryFixTests
{
	[Fact]
	public async Task FixAll_From_X1001()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void [|TestMethod1|](int a) { }

				[Fact]
				public void [|TestMethod2|](string b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void TestMethod1(int a) { }

				[Theory]
				public void TestMethod2(string b) { }
			}
			""";

		await Verify_X1001.VerifyCodeFixFixAll(before, after, ConvertToTheoryFix.Key_ConvertToTheory);
	}

	[Fact]
	public async Task FixAll_From_X1005()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				[InlineData(1)]
				public void [|TestMethod1|]() { }

				[Fact]
				[InlineData("hello")]
				public void [|TestMethod2|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void TestMethod1() { }

				[Theory]
				[InlineData("hello")]
				public void TestMethod2() { }
			}
			""";

		await Verify_X1005.VerifyCodeFixFixAll(before, after, ConvertToTheoryFix.Key_ConvertToTheory);
	}
}
