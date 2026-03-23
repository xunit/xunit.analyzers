using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

public class X1000_TestClassMustBePublicFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			class [|TestClass1|] {
				[Fact]
				public void TestMethod() { }
			}

			internal class [|TestClass2|] {
				[Fact]
				public void TestMethod() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass1 {
				[Fact]
				public void TestMethod() { }
			}

			public class TestClass2 {
				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, TestClassMustBePublicFixer.Key_MakeTestClassPublic);
	}
}
