using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck>;

public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckTests
{
	public static TheoryData<string> Methods =
	[
		Constants.Asserts.True,
		Constants.Asserts.False,
	];

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForLinqAnyCheck_Triggers(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
				void TestMethod() {{
					[|Xunit.Assert.{0}(new [] {{ 1 }}.Any(i => true))|];
				}}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}
}
