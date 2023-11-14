using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class AssertSingleShouldBeUsedForSingleParameterFixerTests
{
	const string beforeTemplate = @"
using Xunit;
using System.Collections.Generic;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        IEnumerable<object> collection = new List<object>() {{ new object() }};

        {0};
    }}
}}";

	const string afterTemplate = @"
using Xunit;
using System.Collections.Generic;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        IEnumerable<object> collection = new List<object>() {{ new object() }};

        {0};
        {1};
    }}
}}";

	[Fact]
	public async void ReplacesCollectionMethod()
	{
		var before = string.Format(beforeTemplate, "[|Assert.Collection(collection, item => Assert.NotNull(item))|]");
		var after = string.Format(afterTemplate, "var item = Assert.Single(collection)", "Assert.NotNull(item)");

		await Verify.VerifyCodeFix(before, after, AssertSingleShouldBeUsedForSingleParameterFixer.Key_UseSingleMethod);
	}
}
