using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;

public class AssertIsTypeShouldNotBeUsedForAbstractTypeFixerTests
{
	const string template = /* lang=c#-test */ """
		using System;
		using Xunit;

		public abstract class TestClass {{
		    [Fact]
		    public void TestMethod() {{
		        var data = new object();

		        {0};
		    }}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "[|Assert.IsType<IDisposable>(data)|]",
		/* lang=c#-test */ "Assert.IsAssignableFrom<IDisposable>(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.IsType<TestClass>(data)|]",
		/* lang=c#-test */ "Assert.IsAssignableFrom<TestClass>(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.IsNotType<IDisposable>(data)|]",
		/* lang=c#-test */ "Assert.IsNotAssignableFrom<IDisposable>(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.IsNotType<TestClass>(data)|]",
		/* lang=c#-test */ "Assert.IsNotAssignableFrom<TestClass>(data)")]
	public async Task Conversions(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertIsTypeShouldNotBeUsedForAbstractTypeFixer.Key_UseAlternateAssert);
	}
}
