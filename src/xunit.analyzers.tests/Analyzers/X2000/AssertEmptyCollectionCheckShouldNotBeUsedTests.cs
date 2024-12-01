using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyCollectionCheckShouldNotBeUsed>;

public class AssertEmptyCollectionCheckShouldNotBeUsedTests
{
	public static TheoryData<string> Collections =
	[
		"new int[0]",
		"new System.Collections.Generic.List<int>()",
		"new System.Collections.Generic.HashSet<int>()",
		"new System.Collections.ObjectModel.Collection<int>()",
		"System.Linq.Enumerable.Empty<int>()",
#if NETCOREAPP3_0_OR_GREATER
		"default(System.Collections.Generic.IAsyncEnumerable<int>)",
#endif
	];

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task CollectionCheckWithoutAction_Triggers(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					[|Xunit.Assert.Collection({0})|];
					[|Xunit.Assert.CollectionAsync({0})|];
				}}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task CollectionCheckWithAction_DoesNotTrigger(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					Xunit.Assert.Collection({0}, i => Xunit.Assert.True(true));
					Xunit.Assert.CollectionAsync({0}, async i => {{ await System.Threading.Tasks.Task.Yield(); Xunit.Assert.True(true); }});
				}}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(source);
	}
}
