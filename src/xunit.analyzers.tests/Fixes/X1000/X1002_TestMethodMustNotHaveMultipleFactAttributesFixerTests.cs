using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

public class X1002_TestMethodMustNotHaveMultipleFactAttributesFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				[Theory]
				public void [|TestMethod1|]() { }

				[Fact]
				[Theory]
				public void [|TestMethod2|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void TestMethod1() { }

				[Theory]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, TestMethodMustNotHaveMultipleFactAttributesFixer.Key_KeepAttribute("Theory"));
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class FactDerivedAttribute : FactAttribute { }

			public class TestClass {
				[Fact]
				[FactDerived]
				public void [|TestMethod1|]() { }

				[Fact]
				[FactDerived]
				public void [|TestMethod2|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class FactDerivedAttribute : FactAttribute { }

			public class TestClass {
				[Fact]
				public void TestMethod1() { }

				[Fact]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAllNonAot(before, after, TestMethodMustNotHaveMultipleFactAttributesFixer.Key_KeepAttribute("Fact"));
	}
}
