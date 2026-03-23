using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class DataAttributeShouldBeUsedOnATheoryFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[InlineData(1)]
				public void [|TestMethod1|]() { }

				[InlineData("hello")]
				public void [|TestMethod2|]() { }
			}
			""";
		var afterMarkAsTheory = /* lang=c#-test */ """
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
		var afterRemoveDataAttributes = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public void TestMethod1() { }

				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, afterMarkAsTheory, DataAttributeShouldBeUsedOnATheoryFixer.Key_MarkAsTheory);
		await Verify.VerifyCodeFixFixAll(before, afterRemoveDataAttributes, DataAttributeShouldBeUsedOnATheoryFixer.Key_RemoveDataAttributes);
	}
}
