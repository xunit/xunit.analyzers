using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixerTests
{
	public static readonly TheoryData<string, string> Assertions = GenerateAssertions();

	static readonly string template = @"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    Task ThrowingMethod() {{
        throw new NotImplementedException();
    }}

    [Fact]{0}
}}";

	static TheoryData<string, string> GenerateAssertions()
	{
		var templates = new (string, string)[]
		{
			("Assert.Throws<Exception>({0})", "Assert.ThrowsAsync<Exception>({0})"),
			("Assert.Throws<ArgumentException>(\"parameter\", {0})", "Assert.ThrowsAsync<ArgumentException>(\"parameter\", {0})"),
		};

		var lambdas = new[]
		{
			"(Func<Task>)ThrowingMethod",
			"() => Task.Delay(0)",
			"(Func<Task>)(async () => await Task.Delay(0))",
			"(Func<Task>)(async () => await Task.Delay(0).ConfigureAwait(false))",
		};

		var assertions = new TheoryData<string, string>();

		foreach ((var assertionTemplate, var replacementTemplate) in templates)
		{
			foreach (var lambda in lambdas)
			{
				var assertion = string.Format(assertionTemplate, lambda);
				var replacement = string.Format(replacementTemplate, lambda);
				assertions.Add(assertion, replacement);
			}
		}

		return assertions;
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInMethod_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        {{|CS0619:[|{assertion}|]|}};
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        await {replacement};
    }}";

		var before = string.Format(template, beforeMethod);
		var after = string.Format(template, afterMethod);

		await Verify.VerifyCodeFix(before, after, AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer.Key_UseAlternateAssert);
	}
}
