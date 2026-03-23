using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.UseCancellationToken>;

public class X1051_UseCancellationTokenTests
{
	[Fact]
	public async ValueTask V2_only()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[Fact]
				public async Task WithoutCancellationToken_DoesNotTrigger() {
					await Task.Delay(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			class NonTestClass {
				public async Task NonTestMethod() {
					await Task.Delay(1);
				}
			}

			class TestClass {
				void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default(CancellationToken)) { }
				void FunctionWithOverload(int _) { }
				void FunctionWithOverload(int _1, CancellationToken _2) { }
				void FunctionWithOverload_NoUpgrade(int _) { }
				void FunctionWithOverload_NoUpgrade(CancellationToken _) { }
				void FunctionWithOverload_Obsolete(int _) { }
				[Obsolete] void FunctionWithOverload_Obsolete(int _1, CancellationToken _2) { }

				[Fact]
				public void Fact_NoCancellationToken_DoesNotTrigger() {
					Thread.Sleep(1);
				}

				[CulturedFact(new[] { "en-US" })]
				public void CulturedFact_NoCancellationToken_DoesNotTrigger() {
					Thread.Sleep(1);
				}

				[Fact]
				public void Fact_WithAnyCancellationToken_DoesNotTrigger() {
					FunctionWithDefaults(42, TestContext.Current.CancellationToken);
					FunctionWithDefaults(42, cancellationToken: TestContext.Current.CancellationToken);
					FunctionWithDefaults(cancellationToken: TestContext.Current.CancellationToken);

					var token = new CancellationTokenSource().Token;

					FunctionWithDefaults(42, token);
					FunctionWithDefaults(42, cancellationToken: token);
					FunctionWithDefaults(cancellationToken: token);
				}

				[CulturedFact(new[] { "en-US" })]
				public void CulturedFact_WithAnyCancellationToken_DoesNotTrigger() {
					FunctionWithDefaults(42, TestContext.Current.CancellationToken);
					FunctionWithDefaults(42, cancellationToken: TestContext.Current.CancellationToken);
					FunctionWithDefaults(cancellationToken: TestContext.Current.CancellationToken);

					var token = new CancellationTokenSource().Token;

					FunctionWithDefaults(42, token);
					FunctionWithDefaults(42, cancellationToken: token);
					FunctionWithDefaults(cancellationToken: token);
				}

				[Fact]
				public void Fact_WithoutCancellationToken_WithoutDirectUpgrade_DoesNotTrigger() {
					FunctionWithOverload_NoUpgrade(42);
				}

				[CulturedFact(new[] { "en-US" })]
				public void CulturedFact_WithoutCancellationToken_WithoutDirectUpgrade_DoesNotTrigger() {
					FunctionWithOverload_NoUpgrade(42);
				}

				[Fact]
				public void Fact_WithoutCancellationToken_Triggers() {
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

				[CulturedFact(new[] { "en-US" })]
				public void CulturedFact_WithoutCancellationToken_Triggers() {
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

				[Fact]
				public void Fact_InsideLambda_DoesNotTrigger() {
					async Task InnerMethod() {
						await Task.Delay(1);
					}
					Func<Task> _ = async () => await Task.Delay(1);
				}

				[CulturedFact(new[] { "en-US" })]
				public void CulturedFact_InsideLambda_DoesNotTrigger() {
					async Task InnerMethod() {
						await Task.Delay(1);
					}
					Func<Task> _ = async () => await Task.Delay(1);
				}

				[Fact]
				public async ValueTask Fact_InsideAssertionLambda_Triggers() {
					await Assert.CollectionAsync(Array.Empty<int>(), x => [|Task.Delay(x)|], x => [|Task.Delay(x)|]);
					await Assert.ThrowsAsync<Exception>(() => [|Task.Delay(1)|]);
					await Record.ExceptionAsync(() => [|Task.Delay(1)|]);
				}

				[CulturedFact(new[] { "en-US" })]
				public async ValueTask CulturedFact_InsideAssertionLambda_Triggers() {
					await Assert.CollectionAsync(Array.Empty<int>(), x => [|Task.Delay(x)|], x => [|Task.Delay(x)|]);
					await Assert.ThrowsAsync<Exception>(() => [|Task.Delay(1)|]);
					await Record.ExceptionAsync(() => [|Task.Delay(1)|]);
				}

				[Fact]
				public void Fact_WhenOverloadIsObsolete_DoesNotTrigger() {
					FunctionWithOverload_Obsolete(42);
				}

				[CulturedFact(new[] { "en-US" })]
				public void CulturedFact_WhenOverloadIsObsolete_DoesNotTrigger() {
					FunctionWithOverload_Obsolete(42);
				}
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source);
	}
}
