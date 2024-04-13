using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyCollectionCheckShouldNotBeUsed>;

public class AssertEmptyCollectionCheckShouldNotBeUsedTests
{
	public static TheoryData<string> Collections = new()
	{
		"new int[0]",
		"new System.Collections.Generic.List<int>()",
		"new System.Collections.Generic.HashSet<int>()",
		"new System.Collections.ObjectModel.Collection<int>()",
		"System.Linq.Enumerable.Empty<int>()",
#if NETCOREAPP3_0_OR_GREATER
		"default(System.Collections.Generic.IAsyncEnumerable<int>)",
#endif
	};

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task FindsWarningForCollectionCheckWithoutAction(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.Collection({collection})|];
        [|Xunit.Assert.CollectionAsync({collection})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task DoesNotFindWarningForCollectionCheckWithAction(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Collection({collection}, i => Xunit.Assert.True(true));
        Xunit.Assert.CollectionAsync({collection}, async i => {{ await System.Threading.Tasks.Task.Yield(); Xunit.Assert.True(true); }});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}
}
