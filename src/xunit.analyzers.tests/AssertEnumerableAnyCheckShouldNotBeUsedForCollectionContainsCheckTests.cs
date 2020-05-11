using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck>;

namespace Xunit.Analyzers
{
	public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckTests
	{
		public static TheoryData<string> BooleanMethods
			= new TheoryData<string> { "True", "False" };

		[Theory]
		[MemberData(nameof(BooleanMethods))]
		public async void FindsWarning_ForLinqAnyCheck(string method)
		{
			var source =
				@"using System.Linq;
class TestClass { void TestMethod() {
    [|Xunit.Assert." + method + @"(new [] { 1 }.Any(i => true))|];
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}
	}
}
