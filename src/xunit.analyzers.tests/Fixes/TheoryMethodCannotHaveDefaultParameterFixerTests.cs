using System;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify_v2_Pre220 = CSharpVerifier<TheoryMethodCannotHaveDefaultParameterFixerTests.Analyzer_v2_Pre220>;

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

		await Verify_v2_Pre220.VerifyCodeFixAsyncV2(before, after);
	}

	internal class Analyzer_v2_Pre220 : TheoryMethodCannotHaveDefaultParameter
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Core(compilation, new Version(2, 1, 999));
	}
}
