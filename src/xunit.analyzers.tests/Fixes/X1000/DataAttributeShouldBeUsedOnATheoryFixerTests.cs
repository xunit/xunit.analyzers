using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class DataAttributeShouldBeUsedOnATheoryFixerTests
{
	[Fact]
	public async Task FixAll_MarkAsTheory()
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

		await Verify.VerifyCodeFixFixAll(before, after, DataAttributeShouldBeUsedOnATheoryFixer.Key_MarkAsTheory);
	}

	[Fact]
	public async Task FixAll_RemoveDataAttributes()
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
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public void TestMethod1() { }

				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, DataAttributeShouldBeUsedOnATheoryFixer.Key_RemoveDataAttributes);
	}
}
