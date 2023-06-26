using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
{
	public static TheoryData<string> NonAsyncLambdas = new()
	{
		"ThrowingMethod",
		"() => 1",
	};

	public class WithTask : AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
	{
		public static TheoryData<string> AsyncLambdas = new()
		{
			"(System.Func<System.Threading.Tasks.Task>)ThrowingMethod",
			"() => System.Threading.Tasks.Task.Delay(0)",
			"(System.Func<System.Threading.Tasks.Task>)(async () => await System.Threading.Tasks.Task.Delay(0))",
			"(System.Func<System.Threading.Tasks.Task>)(async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false))",
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

			await Verify.VerifyAnalyzer(source);
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

			await Verify.VerifyAnalyzer(source);
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

			await Verify.VerifyAnalyzer(source);
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
			var expected = new[]
			{
				DiagnosticResult
					.CompilerError("CS0619")
					.WithSpan(8, 9, 8, 70 + lambda.Length)
					.WithArguments("Xunit.Assert.Throws(System.Type, System.Func<System.Threading.Tasks.Task>)", "You must call Assert.ThrowsAsync (and await the result) when testing async code."),
				Verify
					.Diagnostic()
					.WithSpan(8, 9, 8, 70 + lambda.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync),
			};

			await Verify.VerifyAnalyzer(source, expected);
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

			await Verify.VerifyAnalyzer(source, expected);
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
					.WithArguments("Xunit.Assert.Throws<T>(string?, System.Func<System.Threading.Tasks.Task>)", "You must call Assert.ThrowsAsync<T> (and await the result) when testing async code."),
				Verify
					.Diagnostic()
					.WithSpan(8, 9, 8, 66 + lambda.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync),
			};

			await Verify.VerifyAnalyzer(source, expected);
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

			await Verify.VerifyAnalyzer(source);
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

			await Verify.VerifyAnalyzer(source);
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

			await Verify.VerifyAnalyzer(source);
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
			var expected = new[]
			{
				DiagnosticResult
					.CompilerError("CS0619")
					.WithSpan(8, 9, 8, 65 + lambda.Length)
					.WithArguments("Xunit.Assert.ThrowsAny<T>(System.Func<System.Threading.Tasks.Task>)", "You must call Assert.ThrowsAnyAsync<T> (and await the result) when testing async code."),
				Verify
					.Diagnostic()
					.WithSpan(8, 9, 8, 65 + lambda.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Assert.ThrowsAny()", Constants.Asserts.ThrowsAnyAsync),
			};

			await Verify.VerifyAnalyzer(source, expected);
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

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class WithValueTask : AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
	{
		public static TheoryData<string> AsyncLambdas = new()
		{
			"(System.Func<System.Threading.Tasks.ValueTask>)ThrowingMethod",
			"(System.Func<System.Threading.Tasks.ValueTask>)(async () => await System.Threading.Tasks.Task.Delay(0))",
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

			await Verify.VerifyAnalyzerV3(source);
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

			await Verify.VerifyAnalyzerV3(source);
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

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(AsyncLambdas))]
		public async void Throws_NonGeneric_WithAsyncLambda_FindsDiagnostic(string lambda)
		{
			var source = $@"
class TestClass {{
    System.Threading.Tasks.ValueTask ThrowingMethod() {{
		throw new System.NotImplementedException();
    }}

    void TestMethod() {{
        Xunit.Assert.Throws(typeof(System.NotImplementedException), {lambda});
    }}
}}";
			var expected = new[]
			{
				DiagnosticResult
					.CompilerError("CS0619")
					.WithSpan(8, 9, 8, 70 + lambda.Length)
					.WithArguments("Xunit.Assert.Throws(System.Type, System.Func<System.Threading.Tasks.ValueTask>)", "You must call Assert.ThrowsAsync (and await the result) when testing async code."),
				Verify
					.Diagnostic()
					.WithSpan(8, 9, 8, 70 + lambda.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync),
			};

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(AsyncLambdas))]
		public async void Throws_Generic_WithAsyncLambda_FindsDiagnostic(string lambda)
		{
			var source = $@"
class TestClass {{
    System.Threading.Tasks.ValueTask ThrowingMethod() {{
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
					.WithMessage("'Assert.Throws<T>(Func<ValueTask>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify
					.Diagnostic()
					.WithSpan(8, 9, 8, 62 + lambda.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync),
			};

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(AsyncLambdas))]
		public async void Throws_Generic_WithNamedArgumentException_WithAsyncLambda_FindsDiagnostic(string lambda)
		{
			var source = $@"
class TestClass {{
    System.Threading.Tasks.ValueTask ThrowingMethod() {{
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
					.WithArguments("Xunit.Assert.Throws<T>(string?, System.Func<System.Threading.Tasks.ValueTask>)", "You must call Assert.ThrowsAsync<T> (and await the result) when testing async code."),
				Verify
					.Diagnostic()
					.WithSpan(8, 9, 8, 66 + lambda.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Assert.Throws()", Constants.Asserts.ThrowsAsync),
			};

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(AsyncLambdas))]
		public async void ThrowsAsync_NonGeneric_WithAsyncLambda_FindsNoDiagnostics(string lambda)
		{
			var source = $@"
class TestClass {{
    System.Threading.Tasks.ValueTask ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    async System.Threading.Tasks.Task TestMethod() {{
        await Xunit.Assert.ThrowsAsync(typeof(System.NotImplementedException), {lambda});
    }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(AsyncLambdas))]
		public async void ThrowsAsync_Generic_WithAsyncLambda_FindsNoDiagnostics(string lambda)
		{
			var source = $@"
class TestClass {{
    System.Threading.Tasks.ValueTask ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    async void TestMethod() {{
        await Xunit.Assert.ThrowsAsync<System.NotImplementedException>({lambda});
    }}
}}";

			await Verify.VerifyAnalyzerV3(source);
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

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(AsyncLambdas))]
		public async void ThrowsAny_WithAsyncLambda_FindsDiagnostic(string lambda)
		{
			var source = $@"
class TestClass {{
    System.Threading.Tasks.ValueTask ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    void TestMethod() {{
        Xunit.Assert.ThrowsAny<System.NotImplementedException>({lambda});
    }}
}}";
			var expected = new[]
			{
				DiagnosticResult
					.CompilerError("CS0619")
					.WithSpan(8, 9, 8, 65 + lambda.Length)
					.WithArguments("Xunit.Assert.ThrowsAny<T>(System.Func<System.Threading.Tasks.ValueTask>)", "You must call Assert.ThrowsAnyAsync<T> (and await the result) when testing async code."),
				Verify
					.Diagnostic()
					.WithSpan(8, 9, 8, 65 + lambda.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Assert.ThrowsAny()", Constants.Asserts.ThrowsAnyAsync),
			};

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(AsyncLambdas))]
		public async void ThrowsAnyAsync_WithAsyncLambda_FindsNoDiagnostics(string lambda)
		{
			var source = $@"
class TestClass {{
    System.Threading.Tasks.ValueTask ThrowingMethod() {{
        throw new System.NotImplementedException();
    }}

    async void TestMethod() {{
        await Xunit.Assert.ThrowsAnyAsync<System.NotImplementedException>({lambda});
    }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}
	}
}
