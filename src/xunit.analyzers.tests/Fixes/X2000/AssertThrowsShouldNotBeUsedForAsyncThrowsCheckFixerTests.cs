using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixerTests
{
	public static readonly TheoryData<string, string> Assertions = GenerateAssertions();

	const string template = /* lang=c#-test */ """
		using System;
		using System.Threading.Tasks;
		using Xunit;

		public class TestClass {{
			Task ThrowingMethod() {{
				throw new NotImplementedException();
			}}

			[Fact]{0}
		}}
		""";

	static TheoryData<string, string> GenerateAssertions()
	{
		var templates = new (string, string)[]
		{
			(
				/* lang=c#-test */ "Assert.Throws(typeof(Exception), {0})",
				/* lang=c#-test */ "Assert.ThrowsAsync(typeof(Exception), {0})"
			),
			(
				/* lang=c#-test */ "Assert.Throws<Exception>({0})",
				/* lang=c#-test */ "Assert.ThrowsAsync<Exception>({0})"
			),
			(
				/* lang=c#-test */ "Assert.Throws<ArgumentException>(\"parameter\", {0})",
				/* lang=c#-test */ "Assert.ThrowsAsync<ArgumentException>(\"parameter\", {0})"
			),
			(
				/* lang=c#-test */ "Assert.ThrowsAny<Exception>({0})",
				/* lang=c#-test */ "Assert.ThrowsAnyAsync<Exception>({0})"
			),
		};

		var lambdas = new[]
		{
			/* lang=c#-test */ "(Func<Task>)ThrowingMethod",
			/* lang=c#-test */ "() => Task.Delay(0)",
			/* lang=c#-test */ "(Func<Task>)(async () => await Task.Delay(0))",
			/* lang=c#-test */ "(Func<Task>)(async () => await Task.Delay(0).ConfigureAwait(false))",
		};

		var assertions = new TheoryData<string, string>();

		foreach ((var assertionTemplate, var replacementTemplate) in templates)
			foreach (var lambda in lambdas)
			{
				var assertion = string.Format(assertionTemplate, lambda);
				var replacement = string.Format(replacementTemplate, lambda);
				assertions.Add(assertion, replacement);
			}

		return assertions;
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInMethod_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					{{|CS0619:[|{0}|]|}};
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					await {0};
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInInvokedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Func<int> function = () => {{
						{{|CS0619:[|{0}|]|}};
						return 0;
					}};

					int number = function();
					function();
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					Func<Task<int>> function = async () => {{
						await {0};
						return 0;
					}};

					int number = {{|CS0029:function()|}};
					function();
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInInvokedAnonymousFunctionWithAssignment_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Func<int> function = () => 0;
					function = () => {{
						{{|CS0619:[|{0}|]|}};
						return 0;
					}};

					int number = function();
					function();
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					Func<Task<int>> function = () => {{|CS0029:{{|CS1662:0|}}|}};
					function = async () => {{
						await {0};
						return 0;
					}};

					int number = {{|CS0029:function()|}};
					function();
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

#if ROSLYN_LATEST  // C# 10 is required for anonymous lambda types

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInInvokedAnonymousFunctionWithVar_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					var function = () => {{
						{{|CS0619:[|{0}|]|}};
						return 0;
					}};

					int number = function();
					function();
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					var function = async () => {{
						await {0};
						return 0;
					}};

					int number = {{|CS0029:function()|}};
					function();
				}}
			""", replacement);

		await VerifyCodeFix(LanguageVersion.CSharp10, beforeMethod, afterMethod);
	}

#endif

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInUninvokedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Func<int> function = () => {{
						{{|CS0619:[|{0}|]|}};
						return 0;
					}};
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Func<Task<int>> function = async () => {{
						await {0};
						return 0;
					}};
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInInvokedNestedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Action<int> outerFunction = (number) => {{
						Func<string> innerFunction = delegate () {{
							{{|CS0619:[|{0}|]|}};
							return string.Empty;
						}};

						var message = innerFunction().ToLower();
						innerFunction();
					}};

					outerFunction(0);
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					Func<int, Task> outerFunction = async (number) => {{
						Func<Task<string>> innerFunction = async delegate () {{
							await {0};
							return string.Empty;
						}};

						var message = innerFunction().{{|CS7036:ToLower|}}();
						innerFunction();
					}};

					outerFunction(0);
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInExplicitlyInvokedNestedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Action<int> outerFunction = (number) => {{
						Func<string> innerFunction = delegate () {{
							{{|CS0619:[|{0}|]|}};
							return string.Empty;
						}};

						var message = innerFunction.Invoke().ToLower();
					}};

					outerFunction.Invoke(0);
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					Func<int, Task> outerFunction = async (number) => {{
						Func<Task<string>> innerFunction = async delegate () {{
							await {0};
							return string.Empty;
						}};

						var message = innerFunction.Invoke().{{|CS7036:ToLower|}}();
					}};

					outerFunction.Invoke(0);
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInConditionallyInvokedNestedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Action<int> outerFunction = (number) => {{
						Func<string> innerFunction = delegate () {{
							{{|CS0619:[|{0}|]|}};
							return string.Empty;
						}};

						var message = innerFunction?.Invoke().ToLower();
					}};

					outerFunction?.Invoke(0);
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					Func<int, Task> outerFunction = async (number) => {{
						Func<Task<string>> innerFunction = async delegate () {{
							await {0};
							return string.Empty;
						}};

						var message = innerFunction?.Invoke().{{|CS7036:ToLower|}}();
					}};

					outerFunction?.Invoke(0);
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInUninvokedNestedAnonymousFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Action<int> outerFunction = (number) => {{
						Func<string> innerFunction = () => {{
							{{|CS0619:[|{0}|]|}};
							return string.Empty;
						}};
					}};
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					Action<int> outerFunction = (number) => {{
						Func<Task<string>> innerFunction = async () => {{
							await {0};
							return string.Empty;
						}};
					}};
				}}
			""", replacement);

		await VerifyCodeFix(beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInInvokedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					int number = Function();
					Function();

					int Function() {{
						{{|CS0619:[|{0}|]|}};
						return 0;
					}}
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					int number = {{|CS0029:Function()|}};
					Function();

					async Task<int> Function() {{
						await {0};
						return 0;
					}}
				}}
			""", replacement);

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInUninvokedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					int Function() {{
						{{|CS0619:[|{0}|]|}};
						return 0;
					}}
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					async Task<int> Function() {{
						await {0};
						return 0;
					}}
				}}
			""", replacement);

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInInvokedNestedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					int number = OuterFunction();
					OuterFunction();

					int OuterFunction() {{
						var message = InnerFunction().ToLower();
						InnerFunction();
						return 0;

						string InnerFunction() {{
							{{|CS0619:[|{0}|]|}};
							return string.Empty;
						}}
					}}
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					int number = {{|CS0029:OuterFunction()|}};
					OuterFunction();

					async Task<int> OuterFunction() {{
						var message = InnerFunction().{{|CS7036:ToLower|}}();
						InnerFunction();
						return 0;

						async Task<string> InnerFunction() {{
							await {0};
							return string.Empty;
						}}
					}}
				}}
			""", replacement);

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInUninvokedNestedLocalFunction_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					int OuterFunction() {{
						return 0;

						string InnerFunction() {{
							{{|CS0619:[|{0}|]|}};
							return string.Empty;
						}}
					}}
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					int OuterFunction() {{
						return 0;

						async Task<string> InnerFunction() {{
							await {0};
							return string.Empty;
						}}
					}}
				}}
			""", replacement);

		await VerifyCodeFix(LanguageVersion.CSharp7, beforeMethod, afterMethod);
	}

	[Theory]
	[MemberData(nameof(Assertions))]
	public async Task GivenAssertionInMixedNestedFunctions_ReplacesWithAsyncAssertion(
		string assertion,
		string replacement)
	{
		var beforeMethod = string.Format(/* lang=c#-test */ """
				public void TestMethod() {{
					int OuterLocalFunction() {{
						Func<bool> outerAnonymousFunction = () => {{
							string InnerLocalFunction() {{
								Action innerAnonymousFunction = () => {{
									{{|CS0619:[|{0}|]|}};
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
				}}
			""", assertion);
		var afterMethod = string.Format(/* lang=c#-test */ """
				public async Task TestMethod() {{
					async Task<int> OuterLocalFunction() {{
						Func<Task<bool>> outerAnonymousFunction = async () => {{
							async Task<string> InnerLocalFunction() {{
								Func<Task> innerAnonymousFunction = async () => {{
									await {0};
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
				}}
			""", replacement);

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
