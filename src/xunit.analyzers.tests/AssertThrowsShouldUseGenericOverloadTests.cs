using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

namespace Xunit.Analyzers
{
	public class AssertThrowsShouldUseGenericOverloadTests
	{
		public static TheoryData<string> Methods
			= new TheoryData<string> { "Throws", "ThrowsAsync" };

		[Theory]
		[MemberData(nameof(Methods))]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingMethod(string method)
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert." + method + @"(typeof(System.NotImplementedException), ThrowingMethod);
} }";

			var expected = Verify.Diagnostic().WithSpan(7, 5, 7, 74 + method.Length).WithSeverity(DiagnosticSeverity.Warning);
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[MemberData(nameof(Methods))]
		public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingLambda(string method)
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0));
} }";

			var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 102 + method.Length).WithSeverity(DiagnosticSeverity.Warning);
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Fact]
		public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethod()
		{
			var source =
				@"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(ThrowingMethod);
} }";

			var expected = Verify.CompilerError("CS0619").WithSpan(7, 5, 7, 72).WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'");
			await Verify.VerifyAnalyzerAsync(source, expected);
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
		public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambda()
		{
			var source =
				@"class TestClass { void TestMethod() {
    Xunit.Assert.Throws<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }";

			var expected = Verify.CompilerError("CS0619").WithSpan(2, 5, 2, 100).WithMessage("'Assert.Throws<T>(Func<Task>)' is obsolete: 'You must call Assert.ThrowsAsync<T> (and await the result) when testing async code.'");
			await Verify.VerifyAnalyzerAsync(source, expected);
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
	}
}
