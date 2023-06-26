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
	};

	[Theory]
	[MemberData(nameof(Collections))]
	public async void FindsWarningForCollectionCheckWithoutAction(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.Collection({collection})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async void DoesNotFindWarningForCollectionCheckWithAction(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Collection({collection}, i => Xunit.Assert.True(true));
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}
}
