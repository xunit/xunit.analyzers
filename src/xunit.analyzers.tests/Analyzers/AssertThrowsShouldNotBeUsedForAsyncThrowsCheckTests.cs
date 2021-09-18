using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
{
	public static TheoryData<string> AsyncLambdas = new()
	{
		"ThrowingMethod",
		"() => System.Threading.Tasks.Task.Delay(0)",
		"async () => await System.Threading.Tasks.Task.Delay(0)",
		"async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false)",
	};
	public static TheoryData<string> NonAsyncLambdas = new()
	{
		"ThrowingMethod",
		"() => 1",
	};

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async void Throws_NonGeneric_WithNonAsyncLambda_FindsNoDiagnostics(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Action ThrowingMethod = () => {{
        throw new System.NotImplementedException();
    }};

    void TestMethod() {{
        Xunit.Assert.Throws(typeof(System.NotImplementedException), {lambda});
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async void Throws_Generic_WithNonAsyncLambda_FindsNoDiagnostics(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Action ThrowingMethod = () => {{
        throw new System.NotImplementedException();
    }};

    void TestMethod() {{
        Xunit.Assert.Throws<System.NotImplementedException>({lambda});
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async void Throws_Generic_WithNamedArgumentException_WithNonAsyncLambda_FindsNoDiagnostics(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Action ThrowingMethod = () => {{
        throw new System.NotImplementedException();
    }};

    void TestMethod() {{
        Xunit.Assert.Throws<System.ArgumentException>(""param1"", {lambda});
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async void Throws_NonGeneric_WithAsyncLambda_FindsDiagnostic(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
		throw new System.NotImplementedException();
    }}

    void TestMethod() {{
        Xunit.Assert.Throws(typeof(System.NotImplementedException), {lambda});
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 70 + lambda.Length)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync);

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async void Throws_Generic_WithAsyncLambda_FindsDiagnostic(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    void TestMethod() {{
        Xunit.Assert.Throws<System.NotImplementedException>({lambda});
    }}
}}";
		var expected = new[]
		{
			Verify
				.CompilerError("CS0619")
				.WithSpan(8, 9, 8, 62 + lambda.Length)
				.WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 62 + lambda.Length)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync),
		};

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async void Throws_Generic_WithNamedArgumentException_WithAsyncLambda_FindsDiagnostic(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    void TestMethod() {{
        Xunit.Assert.Throws<System.ArgumentException>(""param1"", {lambda});
    }}
}}";
		var expected = new[]
		{
			Verify
				.CompilerError("CS0619")
				.WithSpan(8, 9, 8, 66 + lambda.Length)
				.WithMessage("'Assert.Throws<T>(string, Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 66 + lambda.Length)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync),
		};

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async void ThrowsAsync_NonGeneric_WithAsyncLambda_FindsNoDiagnostics(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    async System.Threading.Tasks.Task TestMethod() {{
        await Xunit.Assert.ThrowsAsync(typeof(System.NotImplementedException), {lambda});
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async void ThrowsAsync_Generic_WithAsyncLambda_FindsNoDiagnostics(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    async void TestMethod() {{
        await Xunit.Assert.ThrowsAsync<System.NotImplementedException>({lambda});
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(NonAsyncLambdas))]
	public async void ThrowsAny_WithNonAsyncLambda_FindsNoDiagnostics(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Action ThrowingMethod = () => {{
        throw new System.NotImplementedException();
    }};

    void TestMethod() {{
        Xunit.Assert.ThrowsAny<System.NotImplementedException>({lambda});
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async void ThrowsAny_WithAsyncLambda_FindsDiagnostic(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    void TestMethod() {{
        Xunit.Assert.ThrowsAny<System.NotImplementedException>({lambda});
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(8, 9, 8, 65 + lambda.Length)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("Assert.ThrowsAny()", Constants.Asserts.ThrowsAnyAsync);

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncLambdas))]
	public async void ThrowsAnyAsync_WithAsyncLambda_FindsNoDiagnostics(string lambda)
	{
		var source = $@"
class TestClass {{
    System.Threading.Tasks.Task ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    async void TestMethod() {{
        await Xunit.Assert.ThrowsAnyAsync<System.NotImplementedException>({lambda});
    }}
}}";

		await Verify.VerifyAnalyzerAsync(source);
	}
}
