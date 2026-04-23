using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodWithTimeoutShouldUseCancellationToken>;

public class X1069_TestMethodWithTimeoutShouldUseCancellationTokenTests
{
	[Fact]
	public async ValueTask V2_only()
	{
		var source = /* lang=c#-test */ """
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
				public async Task NonTestMethod() {
					await Task.Delay(1);
				}
			}

			public class Facts {
				[Fact]
				public void NoTimeout() {
					Thread.Sleep(1);
				}

				[Fact(Timeout = 0)]
				public void ZeroTimeout() {
					Thread.Sleep(1);
				}

				[Fact(Timeout = 1000)]
				public async Task WithoutCancellationToken() {
					await Task.Delay(1);
				}
			}

			public class Theories {
				[Theory]
				public void NoTimeout() {
					Thread.Sleep(1);
				}

				[Theory(Timeout = 0)]
				public void ZeroTimeout() {
					Thread.Sleep(1);
				}

				[Theory(Timeout = 1000)]
				public async Task WithoutCancellationToken() {
					await Task.Delay(1);
				}
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
				public async Task NonTestMethod() {
					await Task.Delay(1);
				}
			}

			public class Facts {
				[Fact]
				public void NoTimeout() {
					Thread.Sleep(1);
				}

				[Fact(Timeout = 0)]
				public void ZeroTimeout() {
					Thread.Sleep(1);
				}

				[Fact({|#0:Timeout = 1000|})]
				public async Task WithoutCancellationToken() {
					await Task.Delay(1);
				}

				[Fact(Timeout = 1000)]
				public async Task WithDirectCancellationToken() {
					await Task.Delay(1, TestContext.Current.CancellationToken);
				}

				[Fact(Timeout = 1000)]
				public async Task WithLocalCancellationToken() {
					var token = TestContext.Current.CancellationToken;
					await Task.Delay(1, token);
				}

				[Fact(Timeout = 1000)]
				public async Task WithCancellationTokenInLambda() {
					await Assert.ThrowsAsync<OperationCanceledException>(async () => await Task.Delay(1000, TestContext.Current.CancellationToken));
				}
			}

			public class CulturedFacts {
				[CulturedFact(new[] { "en-US" })]
				public void NoTimeout() {
					Thread.Sleep(1);
				}

				[CulturedFact(new[] { "en-US" }, Timeout = 0)]
				public void ZeroTimeout() {
					Thread.Sleep(1);
				}

				[CulturedFact(new[] { "en-US" }, {|#1:Timeout = 1000|})]
				public async Task WithoutCancellationToken() {
					await Task.Delay(1);
				}

				[CulturedFact(new[] { "en-US" }, Timeout = 1000)]
				public async Task WithDirectCancellationToken() {
					await Task.Delay(1, TestContext.Current.CancellationToken);
				}

				[CulturedFact(new[] { "en-US" }, Timeout = 1000)]
				public async Task WithLocalCancellationToken() {
					var token = TestContext.Current.CancellationToken;
					await Task.Delay(1, token);
				}

				[CulturedFact(new[] { "en-US" }, Timeout = 1000)]
				public async Task WithCancellationTokenInLambda() {
					await Assert.ThrowsAsync<OperationCanceledException>(async () => await Task.Delay(1000, TestContext.Current.CancellationToken));
				}
			}

			public class Theories {
				[Theory]
				public void NoTimeout() {
					Thread.Sleep(1);
				}

				[Theory(Timeout = 0)]
				public void ZeroTimeout() {
					Thread.Sleep(1);
				}

				[Theory({|#2:Timeout = 1000|})]
				public async Task WithoutCancellationToken() {
					await Task.Delay(1);
				}

				[Theory(Timeout = 1000)]
				public async Task WithDirectCancellationToken() {
					await Task.Delay(1, TestContext.Current.CancellationToken);
				}

				[Theory(Timeout = 1000)]
				public async Task WithLocalCancellationToken() {
					var token = TestContext.Current.CancellationToken;
					await Task.Delay(1, token);
				}

				[Theory(Timeout = 1000)]
				public async Task WithCancellationTokenInLambda() {
					await Assert.ThrowsAsync<OperationCanceledException>(async () => await Task.Delay(1000, TestContext.Current.CancellationToken));
				}
			}

			public class CulturedTheories {
				[CulturedTheory(new[] { "en-US" })]
				public void NoTimeout() {
					Thread.Sleep(1);
				}

				[CulturedTheory(new[] { "en-US" }, Timeout = 0)]
				public void ZeroTimeout() {
					Thread.Sleep(1);
				}

				[CulturedFact(new[] { "en-US" }, {|#3:Timeout = 1000|})]
				public async Task WithoutCancellationToken() {
					await Task.Delay(1);
				}

				[CulturedTheory(new[] { "en-US" }, Timeout = 1000)]
				public async Task WithDirectCancellationToken() {
					await Task.Delay(1, TestContext.Current.CancellationToken);
				}

				[CulturedTheory(new[] { "en-US" }, Timeout = 1000)]
				public async Task WithLocalCancellationToken() {
					var token = TestContext.Current.CancellationToken;
					await Task.Delay(1, token);
				}

				[CulturedTheory(new[] { "en-US" }, Timeout = 1000)]
				public async Task WithCancellationTokenInLambda() {
					await Assert.ThrowsAsync<OperationCanceledException>(async () => await Task.Delay(1000, TestContext.Current.CancellationToken));
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("WithoutCancellationToken"),
			Verify.Diagnostic().WithLocation(1).WithArguments("WithoutCancellationToken"),
			Verify.Diagnostic().WithLocation(2).WithArguments("WithoutCancellationToken"),
			Verify.Diagnostic().WithLocation(3).WithArguments("WithoutCancellationToken"),
		};

		await Verify.VerifyAnalyzerV3(source, expected);
	}

	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using System.Runtime.CompilerServices;
			using System.Threading.Tasks;
			using Xunit;

			public sealed class MyFactAttribute : FactAttribute {
				public MyFactAttribute(
					[CallerFilePath] string sourceFilePath = "",
					[CallerLineNumber] int sourceLineNumber = -1)
						: base(sourceFilePath, sourceLineNumber)
				{ }
			}

			public class TestClass {
				[MyFact({|#0:Timeout = 1000|})]
				public async Task TestMethod() {
					await Task.Delay(1);
				}
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod");

		await Verify.VerifyAnalyzerV3NonAot(source, expected);
	}
}
