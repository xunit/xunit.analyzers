using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldNotBeUsedForAsyncThrowsCheck>;

namespace Xunit.Analyzers
{
	public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckTests
	{
		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingMethod()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), ThrowingMethod);
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(7, 5, 7, 80).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.Throws()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 108).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.Throws()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnAsyncThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), async () => await System.Threading.Tasks.Task.Delay(0));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 120).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.Throws()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnAsyncThrowingLambda_ConfiguredTaskAwaitable()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 142).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.Throws()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethod()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(ThrowingMethod);
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(7, 5, 7, 72).WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(7, 5, 7, 72).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 100).WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(2, 5, 2, 100).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnAsyncThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(async () => await System.Threading.Tasks.Task.Delay(0));
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 112).WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(2, 5, 2, 112).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnAsyncThrowingLambda_ConfiguredTaskAwaitable()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false));
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 134).WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(2, 5, 2, 134).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethodWithParamName()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", ThrowingMethod);
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(7, 5, 7, 76).WithMessage("'Assert.Throws<T>(string, Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(7, 5, 7, 76).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambdaWithParamName()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", () => System.Threading.Tasks.Task.Delay(0));
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 104).WithMessage("'Assert.Throws<T>(string, Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(2, 5, 2, 104).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambdaWithParamName_ConfiguredTaskAwaitable()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", () => System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 126).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.Throws()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnAsyncThrowingLambdaWithParamName()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", async () => await System.Threading.Tasks.Task.Delay(0));
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 116).WithMessage("'Assert.Throws<T>(string, Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(2, 5, 2, 116).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionTypeArgument_OnAsyncThrowingLambdaWithParamName_ConfiguredTaskAwaitable()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.ArgumentException>(""param1"", async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false));
} }";

			DiagnosticResult[] expected =
			{
				Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 138).WithMessage("'Assert.Throws<T>(string, Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'"),
				Verify.Diagnostic("xUnit2019").WithSpan(2, 5, 2, 138).WithSeverity(DiagnosticSeverity.Hidden).WithArguments("Assert.Throws()"),
			};
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnThrowingMethod()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(ThrowingMethod);
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(7, 5, 7, 75).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.ThrowsAny()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 103).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.ThrowsAny()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnThrowingLambda_ConfiguredTaskAwaitable()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 125).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.ThrowsAny()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnAsyncThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(async () => await System.Threading.Tasks.Task.Delay(0));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 115).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.ThrowsAny()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async Task FindsWarning_ForThrowsAnyCheck_WithExceptionTypeArgument_OnAsyncThrowingLambda_ConfiguredTaskAwaitable()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.ThrowsAny<System.NotImplementedException>(async () => await System.Threading.Tasks.Task.Delay(0).ConfigureAwait(false));
} }";

			var expected = Verify.Diagnostic("xUnit2014").WithSpan(2, 5, 2, 137).WithSeverity(DiagnosticSeverity.Error).WithArguments("Assert.ThrowsAny()");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionParameter_OnNonAsyncThrowingMethod()
		{
			var source =
				@"class TestClass {
void ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), ThrowingMethod);
} }";

			var expected = Verify.CompilerError("CS0121").WithSpan(7, 18, 7, 24).WithMessage("The call is ambiguous between the following methods or properties: 'Assert.Throws(Type, Action)' and 'Assert.Throws(Type, Func<object>)'");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionParameter_OnNonAsyncThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws(typeof(System.NotImplementedException), () => 1);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}


		[Fact]
		public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionParameter_OnThrowingMethod()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync(typeof(System.NotImplementedException), ThrowingMethod);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionParameter_OnThrowingLambda()
		{
			var source =
				@"class TestClass { async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0));
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingMethod()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync<System.NotImplementedException>(ThrowingMethod);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingLambda()
		{
			var source =
				@"class TestClass { async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAsync<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsAnyAsyncCheck_WithExceptionTypeArgument_OnThrowingMethod()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAnyAsync<System.NotImplementedException>(ThrowingMethod);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsAnyAsyncCheck_WithExceptionTypeArgument_OnThrowingLambda()
		{
			var source =
				@"class TestClass { async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert.ThrowsAnyAsync<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}
	}
}
