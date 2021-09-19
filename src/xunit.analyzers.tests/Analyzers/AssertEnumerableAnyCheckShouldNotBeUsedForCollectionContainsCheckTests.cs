using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck>;

public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckTests
{
	public static TheoryData<string> Methods = new()
	{
		Constants.Asserts.True,
		Constants.Asserts.False,
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarning_ForLinqAnyCheck(string method)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.{method}(new [] {{ 1 }}.Any(i => true))|];
    }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}
}
