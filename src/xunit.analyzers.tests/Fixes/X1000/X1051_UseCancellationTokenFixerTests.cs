using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.UseCancellationToken>;

public class X1051_UseCancellationTokenFixerTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task TaskDelay_Once()
				{
					await [|Task.Delay(1)|];
				}

				[Fact]
				public async Task TaskDelay_Twice()
				{
					await [|Task.Delay(1)|];
					await [|Task.Delay(2)|];
				}

				[Fact]
				public void WithOverload()
				{
					[|FunctionWithOverload(42)|];
					[|FunctionWithOverload(42, default(CancellationToken))|];
				}

				void FunctionWithOverload(int _) { }
				void FunctionWithOverload(int _1, CancellationToken _2) { }

				[Fact]
				public void WithDefaults()
				{
					[|FunctionWithDefaults()|];
					[|FunctionWithDefaults(42)|];
					[|FunctionWithDefaults(cancellationToken: default(CancellationToken))|];
					[|FunctionWithDefaults(42, cancellationToken: default(CancellationToken))|];
				}

				void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default(CancellationToken)) { }

				[Fact]
				public void WithParams()
				{
					[|FunctionWithParams(1, 2, 3)|];
					[|FunctionWithParams()|];
				}

				void FunctionWithParams(params int[] integers) { }
				void FunctionWithParams(int[] integers, CancellationToken token = default(CancellationToken)) { }

				[Fact]
				public void WithNonParamsAndParams()
				{
					[|FunctionWithNonParamsAndParams("hello", Guid.NewGuid(), Guid.NewGuid())|];
				}

				void FunctionWithNonParamsAndParams(string str, params Guid[] guids) { }
				void FunctionWithNonParamsAndParams(string str, Guid[] guids, CancellationToken token = default(CancellationToken)) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Threading;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task TaskDelay_Once()
				{
					await Task.Delay(1, TestContext.Current.CancellationToken);
				}

				[Fact]
				public async Task TaskDelay_Twice()
				{
					await Task.Delay(1, TestContext.Current.CancellationToken);
					await Task.Delay(2, TestContext.Current.CancellationToken);
				}

				[Fact]
				public void WithOverload()
				{
					FunctionWithOverload(42, TestContext.Current.CancellationToken);
					FunctionWithOverload(42, TestContext.Current.CancellationToken);
				}

				void FunctionWithOverload(int _) { }
				void FunctionWithOverload(int _1, CancellationToken _2) { }

				[Fact]
				public void WithDefaults()
				{
					FunctionWithDefaults(cancellationToken: TestContext.Current.CancellationToken);
					FunctionWithDefaults(42, TestContext.Current.CancellationToken);
					FunctionWithDefaults(cancellationToken: TestContext.Current.CancellationToken);
					FunctionWithDefaults(42, cancellationToken: TestContext.Current.CancellationToken);
				}

				void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default(CancellationToken)) { }

				[Fact]
				public void WithParams()
				{
					FunctionWithParams(new int[] { 1, 2, 3 }, TestContext.Current.CancellationToken);
					FunctionWithParams(new int[] { }, TestContext.Current.CancellationToken);
				}

				void FunctionWithParams(params int[] integers) { }
				void FunctionWithParams(int[] integers, CancellationToken token = default(CancellationToken)) { }

				[Fact]
				public void WithNonParamsAndParams()
				{
					FunctionWithNonParamsAndParams("hello", new Guid[] { Guid.NewGuid(), Guid.NewGuid() }, TestContext.Current.CancellationToken);
				}

				void FunctionWithNonParamsAndParams(string str, params Guid[] guids) { }
				void FunctionWithNonParamsAndParams(string str, Guid[] guids, CancellationToken token = default(CancellationToken)) { }
			}
			""";

		await Verify.VerifyCodeFixV3FixAll(before, after, UseCancellationTokenFixer.Key_UseCancellationTokenArgument);
	}
}
