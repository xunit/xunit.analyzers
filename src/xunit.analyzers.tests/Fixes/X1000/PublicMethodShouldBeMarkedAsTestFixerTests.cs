using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

public class PublicMethodShouldBeMarkedAsTestFixerTests
{
	[Fact]
	public async Task FixAll_AddsFactToAllPublicMethodsWithoutParameters()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				public void [|TestMethod2|]() { }

				public void [|TestMethod3|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				[Fact]
				public void TestMethod2() { }

				[Fact]
				public void TestMethod3() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToFact);
	}

	[Fact]
	public async Task FixAll_AddsTheoryToAllPublicMethodsWithParameters()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				public void [|TestMethod2|](int _) { }

				public void [|TestMethod3|](string _) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				[Theory]
				public void TestMethod2(int _) { }

				[Theory]
				public void TestMethod3(string _) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToTheory);
	}

	[Fact]
	public async Task FixAll_MakesAllMethodsInternal()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				public void [|TestMethod2|]() { }

				public void [|TestMethod3|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				internal void TestMethod2() { }

				internal void TestMethod3() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, PublicMethodShouldBeMarkedAsTestFixer.Key_MakeMethodInternal);
	}
}
