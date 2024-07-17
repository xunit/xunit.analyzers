using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class DataAttributeShouldBeUsedOnATheoryFixerTests
{
	[Fact]
	public async Task AddsMissingTheoryAttribute()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [InlineData]
			    public void [|TestMethod|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Theory]
			    [InlineData]
			    public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, DataAttributeShouldBeUsedOnATheoryFixer.Key_MarkAsTheory);
	}

	[Fact]
	public async Task RemovesDataAttributes()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [InlineData]
			    public void [|TestMethod|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, DataAttributeShouldBeUsedOnATheoryFixer.Key_RemoveDataAttributes);
	}
}
