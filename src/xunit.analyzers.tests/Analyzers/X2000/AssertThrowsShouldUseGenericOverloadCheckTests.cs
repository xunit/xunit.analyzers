using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertThrowsShouldUseGenericOverloadCheck>;

public class AssertThrowsShouldUseGenericOverloadCheckTests
{
	public static TheoryData<string> Methods =
	[
		Constants.Asserts.Throws,
		Constants.Asserts.ThrowsAsync,
	];

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForThrowsCheck_WithExceptionParameter_OnThrowingMethod_Triggers(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				System.Threading.Tasks.Task ThrowingMethod() {{
					throw new System.NotImplementedException();
				}}

				void TestMethod() {{
					{{|#0:Xunit.Assert.{0}(typeof(System.NotImplementedException), (System.Func<System.Threading.Tasks.Task>)ThrowingMethod)|}};
				}}
			}}
			""", method);
		var expected = new List<DiagnosticResult> {
			Verify.Diagnostic().WithLocation(0).WithArguments(method, "System.NotImplementedException"),
		};
		if (method == Constants.Asserts.Throws)
			expected.Add(DiagnosticResult.CompilerError("CS0619").WithLocation(0).WithArguments("Xunit.Assert.Throws(System.Type, System.Func<System.Threading.Tasks.Task>)", "You must call Assert.ThrowsAsync (and await the result) when testing async code."));

		await Verify.VerifyAnalyzer(source, [.. expected]);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForThrowsCheck_WithExceptionParameter_OnThrowingLambda_Triggers(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					{{|#0:Xunit.Assert.{0}(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0))|}};
				}}
			}}
			""", method);
		var expected = new List<DiagnosticResult> {
			Verify.Diagnostic().WithLocation(0).WithArguments(method, "System.NotImplementedException"),
		};
		if (method == Constants.Asserts.Throws)
			expected.Add(DiagnosticResult.CompilerError("CS0619").WithLocation(0).WithArguments("Xunit.Assert.Throws(System.Type, System.Func<System.Threading.Tasks.Task>)", "You must call Assert.ThrowsAsync (and await the result) when testing async code."));

		await Verify.VerifyAnalyzer(source, expected.ToArray());
	}

	[Fact]
	public async Task ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethod_TriggersCompilerError()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				System.Threading.Tasks.Task ThrowingMethod() {
					throw new System.NotImplementedException();
				}

				void TestMethod() {
					{|CS0619:Xunit.Assert.Throws<System.NotImplementedException>((System.Func<System.Threading.Tasks.Task>)ThrowingMethod)|};
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				System.Threading.Tasks.Task ThrowingMethod() {
					throw new System.NotImplementedException();
				}

				async System.Threading.Tasks.Task TestMethod() {
					await Xunit.Assert.ThrowsAsync<System.NotImplementedException>((System.Func<System.Threading.Tasks.Task>)ThrowingMethod);
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambda_TriggersCompilerError()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				void TestMethod() {
					{|CS0619:Xunit.Assert.Throws<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0))|};
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForThrowsAsyncCheck_WithExceptionTypeArgument_OnThrowingLambda_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				async System.Threading.Tasks.Task TestMethod() {
					await Xunit.Assert.ThrowsAsync<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
