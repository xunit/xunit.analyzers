using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
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
			("Assert.Throws(typeof(Exception), {0})", "Assert.ThrowsAsync(typeof(Exception), {0})"),
			("Assert.Throws<Exception>({0})", "Assert.ThrowsAsync<Exception>({0})"),
			("Assert.Throws<ArgumentException>(\"parameter\", {0})", "Assert.ThrowsAsync<ArgumentException>(\"parameter\", {0})"),
			("Assert.ThrowsAny<Exception>({0})", "Assert.ThrowsAnyAsync<Exception>({0})"),
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

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInInvokedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        Func<int> function = () => {{
            {{|CS0619:[|{assertion}|]|}};
            return 0;
        }};

        int number = function();
        function();
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        Func<Task<int>> function = async () => {{
            await {replacement};
            return 0;
        }};

        int number = {{|CS0029:function()|}};
        function();
    }}";

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInInvokedAnonymousFunctionWithAssignment_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        Func<int> function = () => 0;
        function = () => {{
            {{|CS0619:[|{assertion}|]|}};
            return 0;
        }};

        int number = function();
        function();
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        Func<Task<int>> function = () => {{|CS0029:{{|CS1662:0|}}|}};
        function = async () => {{
            await {replacement};
            return 0;
        }};

        int number = {{|CS0029:function()|}};
        function();
    }}";

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

#if ROSLYN_4_4_OR_GREATER
	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInInvokedAnonymousFunctionWithVar_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        var function = () => {{
            {{|CS0619:[|{assertion}|]|}};
            return 0;
        }};

        int number = function();
        function();
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        var function = async () => {{
            await {replacement};
            return 0;
        }};

        int number = {{|CS0029:function()|}};
        function();
    }}";

		await VerifyCodeFix(LanguageVersion.CSharp10, beforeMethod, afterMethod);
	}
#endif

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInUninvokedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        Func<int> function = () => {{
            {{|CS0619:[|{assertion}|]|}};
            return 0;
        }};
    }}";

		var afterMethod = $@"
    public void TestMethod() {{
        Func<Task<int>> function = async () => {{
            await {replacement};
            return 0;
        }};
    }}";

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInInvokedNestedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        Action<int> outerFunction = (number) => {{
            Func<string> innerFunction = delegate () {{
                {{|CS0619:[|{assertion}|]|}};
                return string.Empty;
            }};

            var message = innerFunction().ToLower();
            innerFunction();
        }};

        outerFunction(0);
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        Func<int, Task> outerFunction = async (number) => {{
            Func<Task<string>> innerFunction = async delegate () {{
                await {replacement};
                return string.Empty;
            }};

            var message = innerFunction().{{|CS7036:ToLower|}}();
            innerFunction();
        }};

        outerFunction(0);
    }}";

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInUninvokedNestedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        Action<int> outerFunction = (number) => {{
            Func<string> innerFunction = () => {{
                {{|CS0619:[|{assertion}|]|}};
                return string.Empty;
            }};
        }};
    }}";

		var afterMethod = $@"
    public void TestMethod() {{
        Action<int> outerFunction = (number) => {{
            Func<Task<string>> innerFunction = async () => {{
                await {replacement};
                return string.Empty;
            }};
        }};
    }}";

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInInvokedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        int number = Function();
        Function();

        int Function() {{
            {{|CS0619:[|{assertion}|]|}};
            return 0;
        }}
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        int number = {{|CS0029:Function()|}};
        Function();

        async Task<int> Function() {{
            await {replacement};
            return 0;
        }}
    }}";

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInUninvokedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        int Function() {{
            {{|CS0619:[|{assertion}|]|}};
            return 0;
        }}
    }}";

		var afterMethod = $@"
    public void TestMethod() {{
        async Task<int> Function() {{
            await {replacement};
            return 0;
        }}
    }}";

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInInvokedNestedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        int number = OuterFunction();
        OuterFunction();

        int OuterFunction() {{
            var message = InnerFunction().ToLower();
            InnerFunction();
            return 0;

            string InnerFunction() {{
                {{|CS0619:[|{assertion}|]|}};
                return string.Empty;
            }}
        }}
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        int number = {{|CS0029:OuterFunction()|}};
        OuterFunction();

        async Task<int> OuterFunction() {{
            var message = InnerFunction().{{|CS7036:ToLower|}}();
            InnerFunction();
            return 0;

            async Task<string> InnerFunction() {{
                await {replacement};
                return string.Empty;
            }}
        }}
    }}";

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInUninvokedNestedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        int OuterFunction() {{
            return 0;

            string InnerFunction() {{
                {{|CS0619:[|{assertion}|]|}};
                return string.Empty;
            }}
        }}
    }}";

		var afterMethod = $@"
    public void TestMethod() {{
        int OuterFunction() {{
            return 0;

            async Task<string> InnerFunction() {{
                await {replacement};
                return string.Empty;
            }}
        }}
    }}";

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async void GivenAssertionInMixedNestedFunctions_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = $@"
    public void TestMethod() {{
        int OuterLocalFunction() {{
            Func<bool> outerAnonymousFunction = () => {{
                string InnerLocalFunction() {{
                    Action innerAnonymousFunction = () => {{
                        {{|CS0619:[|{assertion}|]|}};
                    }};

                    innerAnonymousFunction();
                    return string.Empty;
                }}

                string message = InnerLocalFunction();
                InnerLocalFunction();
                return false;
            }};

            bool condition = outerAnonymousFunction();
            outerAnonymousFunction();
            return 0;
        }}

        int number = OuterLocalFunction();
        OuterLocalFunction();
    }}";

		var afterMethod = $@"
    public async Task TestMethod() {{
        async Task<int> OuterLocalFunction() {{
            Func<Task<bool>> outerAnonymousFunction = async () => {{
                async Task<string> InnerLocalFunction() {{
                    Func<Task> innerAnonymousFunction = async () => {{
                        await {replacement};
                    }};

                    innerAnonymousFunction();
                    return string.Empty;
                }}

                string message = {{|CS0029:InnerLocalFunction()|}};
                InnerLocalFunction();
                return false;
            }};

            bool condition = {{|CS0029:outerAnonymousFunction()|}};
            outerAnonymousFunction();
            return 0;
        }}

        int number = {{|CS0029:OuterLocalFunction()|}};
        OuterLocalFunction();
    }}";

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	static async Task VerifyCodeFix(
		string beforeMethod,
		string afterMethod)
	{
		var before = string.Format(template, beforeMethod);
		var after = string.Format(template, afterMethod);

		await Verify.VerifyCodeFix(before, after, AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer.Key_UseAlternateAssert);
	}

	static async Task VerifyCodeFix(
		LanguageVersion languageVersion,
		string beforeMethod,
		string afterMethod)
	{
		var before = string.Format(template, beforeMethod);
		var after = string.Format(template, afterMethod);

		await Verify.VerifyCodeFix(languageVersion, before, after, AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer.Key_UseAlternateAssert);
	}
}
