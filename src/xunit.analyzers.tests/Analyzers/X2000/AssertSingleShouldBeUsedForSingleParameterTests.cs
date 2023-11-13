using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class AssertSingleShouldBeUsedForSingleParameterTests
{
	[Fact]
	public async void FindsInfo_ForSingleItemCollectionCheck()
	{
		var code = @"
using Xunit;
using System.Collections.Generic;

public class TestClass {
    [Fact]
    public void TestMethod() {
        IEnumerable<object> collection = new List<object>() { new object() };

        Assert.Collection(collection, item => Assert.NotNull(item));
    }
}";

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(10, 9, 10, 68)
				.WithArguments("Collection");

		await Verify.VerifyAnalyzer(code, expected);
	}

	[Fact]
	public async void DoesNotFindInfo_ForMultipleItemCollectionCheck()
	{
		var code = @"
using Xunit;
using System.Collections.Generic;

public class TestClass {
    [Fact]
    public void TestMethod() {
        IEnumerable<object> collection = new List<object>() { new object(), new object() };

        Assert.Collection(collection, item1 => Assert.NotNull(item1), item2 => Assert.NotNull(item2));
    }
}";

		await Verify.VerifyAnalyzer(code);
	}
}
