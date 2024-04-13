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
	public async Task FindsInfo_ForSingleItemCollectionCheck(string collection)
	{
		var code = @$"
using Xunit;
using System.Collections.Generic;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        Assert.Collection({collection}, item => Assert.NotNull(item));
    }}
}}";

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 58 + collection.Length)
				.WithArguments("Collection");

		await Verify.VerifyAnalyzer(code, expected);
	}

	[Theory]
	[InlineData("default(IEnumerable<int>)")]
#if NETCOREAPP3_0_OR_GREATER
	[InlineData("default(IAsyncEnumerable<int>)")]
#endif
	public async Task DoesNotFindInfo_ForMultipleItemCollectionCheck(string collection)
	{
		var code = @$"
using Xunit;
using System.Collections.Generic;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        Assert.Collection({collection}, item1 => Assert.NotNull(item1), item2 => Assert.NotNull(item2));
    }}
}}";

		await Verify.VerifyAnalyzer(code);
	}
}
