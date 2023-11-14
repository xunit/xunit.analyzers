using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldBeUsedForSingleParameter>;

public class AssertSingleShouldBeUsedForSingleParameterFixerTests
{
	public static TheoryData<string, string> Statements = new()
	{
		{
			"[|Assert.Collection(collection, item => Assert.NotNull(item))|]",
			@"var item = Assert.Single(collection);
        Assert.NotNull(item);"
		},
		{
			"[|Assert.Collection(collection, item => { Assert.NotNull(item); })|]",
			@"var item = Assert.Single(collection);
        Assert.NotNull(item);"
		},
		{
			"[|Assert.Collection(collection, item => { Assert.NotNull(item); Assert.NotNull(item); })|]",
			@"var item = Assert.Single(collection);
        Assert.NotNull(item);
        Assert.NotNull(item);"
		},
		{
			@"[|Assert.Collection(collection, item => {
            Assert.NotNull(item);
            Assert.NotNull(item);
        })|]",
			@"var item = Assert.Single(collection);
        Assert.NotNull(item);
        Assert.NotNull(item);"
		},
	};

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

        {0}
    }}
}}";

	[Theory]
	[MemberData(nameof(Statements))]
	public async void ReplacesCollectionMethod(string statementBefore, string statementAfter)
	{
		var before = string.Format(beforeTemplate, statementBefore);
		var after = string.Format(afterTemplate, statementAfter);

		await Verify.VerifyCodeFix(before, after, AssertSingleShouldBeUsedForSingleParameterFixer.Key_UseSingleMethod);
	}
}
