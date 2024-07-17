using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class AssertSingleShouldBeUsedForSingleParameterTests
{
	[Theory]
	[InlineData("default(IEnumerable<int>)")]
#if NETCOREAPP3_0_OR_GREATER
	[InlineData("default(IAsyncEnumerable<int>)")]
#endif
	public async Task ForSingleItemCollectionCheck_Triggers(string collection)
	{
		var code = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System.Collections.Generic;

			public class TestClass {{
			    [Fact]
			    public void TestMethod() {{
			        {{|#0:Assert.Collection({0}, item => Assert.NotNull(item))|}};
			    }}
			}}
			""", collection);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Collection");

		await Verify.VerifyAnalyzer(code, expected);
	}

	[Theory]
	[InlineData("default(IEnumerable<int>)")]
#if NETCOREAPP3_0_OR_GREATER
	[InlineData("default(IAsyncEnumerable<int>)")]
#endif
	public async Task ForMultipleItemCollectionCheck_DoesNotTrigger(string collection)
	{
		var code = string.Format(/* lang=c#-test */ """
			using Xunit;
			using System.Collections.Generic;

			public class TestClass {{
			    [Fact]
			    public void TestMethod() {{
			        Assert.Collection({0}, item1 => Assert.NotNull(item1), item2 => Assert.NotNull(item2));
			    }}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(code);
	}
}
