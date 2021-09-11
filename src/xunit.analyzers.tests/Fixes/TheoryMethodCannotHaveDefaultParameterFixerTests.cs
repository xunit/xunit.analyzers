using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<TheoryMethodCannotHaveDefaultParameterFixerTests.Analyzer>;

public class TheoryMethodCannotHaveDefaultParameterFixerTests
{
	[Fact]
	public async void RemovesDefaultParameterValue()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int _ [|= 0|]) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int _) { }
}";

		await Verify.VerifyCodeFixAsync(before, after);
	}

	internal class Analyzer : TheoryMethodCannotHaveDefaultParameter
	{
		public Analyzer()
			: base("2.1.99")
		{ }
	}
}
