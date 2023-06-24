using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;

public class AssertIsTypeShouldNotBeUsedForAbstractTypeFixerTests
{
	const string template = @"
using System;
using Xunit;

public abstract class TestClass {{
    [Fact]
    public void TestMethod() {{
        var data = new object();

        {0};
    }}
}}";

	[Theory]
	[InlineData("[|Assert.IsType<IDisposable>(data)|]", "Assert.IsAssignableFrom<IDisposable>(data)")]
	[InlineData("[|Assert.IsType<TestClass>(data)|]", "Assert.IsAssignableFrom<TestClass>(data)")]
	[InlineData("[|Assert.IsNotType<IDisposable>(data)|]", "Assert.IsNotAssignableFrom<IDisposable>(data)")]
	[InlineData("[|Assert.IsNotType<TestClass>(data)|]", "Assert.IsNotAssignableFrom<TestClass>(data)")]
	public async void Conversions(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsyncV2(before, after, AssertIsTypeShouldNotBeUsedForAbstractTypeFixer.Key_UseAlternateAssert);
	}
}
