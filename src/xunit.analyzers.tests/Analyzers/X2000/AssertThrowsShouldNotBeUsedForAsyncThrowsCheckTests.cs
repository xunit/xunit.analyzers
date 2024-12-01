using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
{
	public static TheoryData<string> NonAsyncLambdas =
	[
		"ThrowingMethod",
		"() => 1",
	];

	public static TheoryData<string> AsyncLambdas =
	[
		"(System.Func<System.Threading.Tasks.Task>)ThrowingMethod",
		"() => System.Threading.Tasks.Task.Delay(0)",
		"(System.Func<System.Threading.Tasks.Task>)(async () => await System.Threading.Tasks.Task.Delay(0))",
		"(System.Func<System.Threading.Tasks.Task>)(async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false))",
	];

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async Task Throws_NonGeneric_WithNonAsyncLambda_DoesNotTrigger(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Action ThrowingMethod = () => {{
					throw new System.NotImplementedException();
				}};

				void TestMethod() {{
					Xunit.Assert.Throws(typeof(System.NotImplementedException), {0});
				}}
			}}
			""", lambda);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async Task Throws_Generic_WithNonAsyncLambda_DoesNotTrigger(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Action ThrowingMethod = () => {{
					throw new System.NotImplementedException();
				}};

				void TestMethod() {{
					Xunit.Assert.Throws<System.NotImplementedException>({0});
				}}
			}}
			""", lambda);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async Task Throws_Generic_WithNamedArgumentException_WithNonAsyncLambda_DoesNotTrigger(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Action ThrowingMethod = () => {{
					throw new System.NotImplementedException();
				}};

				void TestMethod() {{
					Xunit.Assert.Throws<System.ArgumentException>("param1", {0});
				}}
			}}
			""", lambda);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async Task Throws_NonGeneric_WithAsyncLambda_Triggers(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				void TestMethod() {{
					{{|#0:{{|CS0619:Xunit.Assert.Throws(typeof(System.NotImplementedException), {0})|}}|}};
				}}
			}}
			""", lambda);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async Task Throws_Generic_WithAsyncLambda_Triggers(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				void TestMethod() {{
					{{|#0:{{|CS0619:Xunit.Assert.Throws<System.NotImplementedException>({0})|}}|}};
				}}
			}}
			""", lambda);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async Task Throws_Generic_WithNamedArgumentException_WithAsyncLambda_Triggers(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				void TestMethod() {{
					{{|#0:{{|CS0619:Xunit.Assert.Throws<System.ArgumentException>("param1", {0})|}}|}};
				}}
			}}
			""", lambda);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async Task ThrowsAsync_NonGeneric_WithAsyncLambda_DoesNotTrigger(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				async System.Threading.Tasks.Task TestMethod() {{
					await Xunit.Assert.ThrowsAsync(typeof(System.NotImplementedException), {0});
				}}
			}}
			""", lambda);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async Task ThrowsAsync_Generic_WithAsyncLambda_DoesNotTrigger(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				async void TestMethod() {{
					await Xunit.Assert.ThrowsAsync<System.NotImplementedException>({0});
				}}
			}}
			""", lambda);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async Task ThrowsAny_WithNonAsyncLambda_DoesNotTrigger(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Action ThrowingMethod = () => {{
					throw new System.NotImplementedException();
				}};

				void TestMethod() {{
					Xunit.Assert.ThrowsAny<System.NotImplementedException>({0});
				}}
			}}
			""", lambda);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async Task ThrowsAny_WithAsyncLambda_Triggers(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				void TestMethod() {{
					{{|#0:{{|CS0619:Xunit.Assert.ThrowsAny<System.NotImplementedException>({0})|}}|}};
				}}
			}}
			""", lambda);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.ThrowsAny()", Constants.Asserts.ThrowsAnyAsync);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async Task ThrowsAnyAsync_WithAsyncLambda_DoesNotTrigger(string lambda)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				async void TestMethod() {{
					await Xunit.Assert.ThrowsAnyAsync<System.NotImplementedException>({0});
				}}
			}}
			""", lambda);

		await Verify.VerifyAnalyzer(source);
	}
}
