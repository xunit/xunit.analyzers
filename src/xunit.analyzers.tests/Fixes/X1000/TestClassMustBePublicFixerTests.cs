using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

public class TestClassMustBePublicFixerTests
{
	[Fact]
	public async Task FixAll_MakesAllClassesPublic()
	{
		var before = /* lang=c#-test */ """
			class [|TestClass1|] {
				[Xunit.Fact]
				public void TestMethod() { }
			}

			internal class [|TestClass2|] {
				[Xunit.Fact]
				public void TestMethod() { }
			}
			""";
		var after = /* lang=c#-test */ """
			public class TestClass1 {
				[Xunit.Fact]
				public void TestMethod() { }
			}

			public class TestClass2 {
				[Xunit.Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, TestClassMustBePublicFixer.Key_MakeTestClassPublic);
	}

	[Fact]
	public async Task ForPartialClassDeclarations_MakesSingleDeclarationPublic()
	{
		var before = /* lang=c#-test */ """
			partial class [|TestClass|] {
				[Xunit.Fact]
				public void TestMethod1() {}
			}

			partial class TestClass {
				[Xunit.Fact]
				public void TestMethod2() {}
			}
			""";
		var after = /* lang=c#-test */ """
			public partial class TestClass {
				[Xunit.Fact]
				public void TestMethod1() {}
			}

			partial class TestClass {
				[Xunit.Fact]
				public void TestMethod2() {}
			}
			""";

		await Verify.VerifyCodeFix(before, after, TestClassMustBePublicFixer.Key_MakeTestClassPublic);
	}
}
