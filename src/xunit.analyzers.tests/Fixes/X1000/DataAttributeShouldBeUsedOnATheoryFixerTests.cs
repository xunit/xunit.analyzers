using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class DataAttributeShouldBeUsedOnATheoryFixerTests
{
	[Fact]
	public async void AddsMissingTheoryAttribute()
	{
		var before = @"
using Xunit;

public class TestClass {
    [InlineData]
    public void [|TestMethod|]() { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFix(before, after, DataAttributeShouldBeUsedOnATheoryFixer.Key_MarkAsTheory);
	}

	[Fact]
	public async void RemovesDataAttributes()
	{
		var before = @"
using Xunit;

public class TestClass {
    [InlineData]
    public void [|TestMethod|]() { }
}";

		var after = @"
using Xunit;

public class TestClass {
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFix(before, after, DataAttributeShouldBeUsedOnATheoryFixer.Key_RemoveDataAttributes);
	}
}
