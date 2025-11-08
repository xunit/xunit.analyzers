using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.UseCancellationToken>;

public class UseCancellationTokenTests
{
	[Fact]
	public async Task NoCancellationToken_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using Xunit;

			class TestClass {
				[Fact]
				public void TestMethod() {
					Thread.Sleep(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task NonTestMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				public async Task NonTestMethod() {
					await Task.Delay(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task WithAnyCancellationToken_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public void TestMethod() {
					FunctionWithDefaults(42, TestContext.Current.CancellationToken);
					FunctionWithDefaults(42, cancellationToken: TestContext.Current.CancellationToken);
					FunctionWithDefaults(cancellationToken: TestContext.Current.CancellationToken);

					var token = new CancellationTokenSource().Token;

					FunctionWithDefaults(42, token);
					FunctionWithDefaults(42, cancellationToken: token);
					FunctionWithDefaults(cancellationToken: token);
				}

				void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default(CancellationToken)) { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task WithoutCancellationToken_V2_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public async Task TestMethod() {
					await Task.Delay(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async Task WithoutCancellationToken_WithoutDirectUpgrade_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public void TestMethod() {
					FunctionWithOverload(42);
				}

				void FunctionWithOverload(int _) { }
				void FunctionWithOverload(CancellationToken _) { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task WithoutCancellationToken_V3_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public void TestMethod() {
					[|FunctionWithDefaults()|];
					[|FunctionWithDefaults(42)|];
					[|FunctionWithDefaults(42, default)|];
					[|FunctionWithDefaults(42, default(CancellationToken))|];
					[|FunctionWithDefaults(cancellationToken: default)|];
					[|FunctionWithDefaults(cancellationToken: default(CancellationToken))|];

					[|FunctionWithOverload(42)|];
					[|FunctionWithOverload(42, default)|];
					[|FunctionWithOverload(42, default(CancellationToken))|];
				}

				void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default) { }

				void FunctionWithOverload(int _) { }
				void FunctionWithOverload(int _1, CancellationToken _2) { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source);
	}

	[Fact]
	public async Task InsideLambda_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public void TestMethod() {
					async Task InnerMethod() {
						await Task.Delay(1);
					}
					Func<Task> _ = async () => await Task.Delay(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7, source);
	}

	[Fact]
	public async Task InsideAssertionLambda_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public async ValueTask TestMethod() {
					await Assert.CollectionAsync(Array.Empty<int>(), x => [|Task.Delay(x)|], x => [|Task.Delay(x)|]);
					await Assert.ThrowsAsync<Exception>(() => [|Task.Delay(1)|]);
					await Record.ExceptionAsync(() => [|Task.Delay(1)|]);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7, source);
	}

	[Fact]
	public async Task WhenOverloadIsObsolete_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public void TestMethod() {
					FunctionWithOverload(42);
				}

				void FunctionWithOverload(int _) {{ }}
				[Obsolete]
				void FunctionWithOverload(int _1, CancellationToken _2) {{ }}
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
