using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

public class X1013_PublicMethodShouldBeMarkedAsTestFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				public void [|TestMethod2|]() { }

				public void [|TestMethod3|](int x) { }
			}
			""";
		var afterFact = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				[Fact]
				public void TestMethod2() { }

				public void {|#0:TestMethod3|}(int x) { }
			}
			""";
		var expectedAfterFact = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod3", "TestClass", "Theory");
		var afterTheory = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				public void {|#0:TestMethod2|}() { }

				[Theory]
				public void TestMethod3(int x) { }
			}
			""";
		var expectedAfterTheory = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod2", "TestClass", "Fact");
		var afterMakeInternal = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() { }

				internal void TestMethod2() { }

				internal void TestMethod3(int x) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, afterFact, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToFact, expectedAfterFact);
		await Verify.VerifyCodeFixFixAll(before, afterTheory, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToTheory, expectedAfterTheory);
		await Verify.VerifyCodeFixFixAll(before, afterMakeInternal, PublicMethodShouldBeMarkedAsTestFixer.Key_MakeMethodInternal);
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
