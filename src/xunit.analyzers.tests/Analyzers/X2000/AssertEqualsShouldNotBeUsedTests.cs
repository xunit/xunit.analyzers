using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualsShouldNotBeUsed>;

public class AssertEqualsShouldNotBeUsedTests
{
	[Theory]
	[InlineData(nameof(object.Equals), Constants.Asserts.Equal)]
	[InlineData(nameof(object.ReferenceEquals), Constants.Asserts.Same)]
	public async Task WhenProhibitedMethodIsUsed_Triggers(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        {{|#0:{{|CS0619:Xunit.Assert.{method}(null, null)|}}|}};
    }}
}}";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
