using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck>;

public class X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckTests
{
	[Fact]
	public async ValueTask V2_andV3()
	{
		var source = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			class TestClass {
				void ForLinqAnyCheck_Triggers() {
					[|Assert.True(new [] { 1 }.Any(i => true))|];
					[|Assert.False(new [] { 1 }.Any(i => true))|];
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
