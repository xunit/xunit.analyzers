using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class X1030_DoNotUseConfigureAwaitTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
				public async Task NonTestMethod_DoesNotTrigger() {
					await Task.Delay(1).ConfigureAwait(false);
				}
			}

			public class TestClass {
				[Fact]
				public async Task NoCall_DoesNotTrigger() {
					await Task.Delay(1);
				}

				[Fact]
				public async Task True_DoesNotTrigger() {
					await Task.Delay(1).ConfigureAwait(true);
				}

				// Invalid values include:
				// - "false" literal
				// - Any computed value
				// - Any variable

				[Fact]
				public async Task InvalidValue_InsideLambda_DoesNotTrigger() {
					var booleanVar = true;
					var t = Task.Run(async () => {
						await Task.Delay(1).ConfigureAwait(false);
						await Task.Delay(1).ConfigureAwait(1 == 2);
						await Task.Delay(1).ConfigureAwait(1 == 1);
						await Task.Delay(1).ConfigureAwait(booleanVar);
					});
					await t;
				}

				[Fact]
				public async Task InvalidValue_InsideLocalFunction_DoesNotTrigger() {
					var booleanVar = true;
					async Task AssertEventStateAsync() {
						await Task.Delay(1).ConfigureAwait(false);
						await Task.Delay(1).ConfigureAwait(1 == 2);
						await Task.Delay(1).ConfigureAwait(1 == 1);
						await Task.Delay(1).ConfigureAwait(booleanVar);
					}
				}

				[Fact]
				public async Task InvalidValue_TaskWithAwait_Triggers() {
					var booleanVar = true;
					await Task.Delay(1).{|#0:ConfigureAwait(false)|};
					await Task.Delay(1).{|#1:ConfigureAwait(1 == 2)|};
					await Task.Delay(1).{|#2:ConfigureAwait(1 == 1)|};
					await Task.Delay(1).{|#3:ConfigureAwait(booleanVar)|};
				}

				[Fact]
				public void InvalidValue_TaskWithoutAwait_Triggers() {
					var booleanVar = true;
					Task.Delay(1).{|#10:ConfigureAwait(false)|}.GetAwaiter().GetResult();
					Task.Delay(1).{|#11:ConfigureAwait(1 == 2)|}.GetAwaiter().GetResult();
					Task.Delay(1).{|#12:ConfigureAwait(1 == 1)|}.GetAwaiter().GetResult();
					Task.Delay(1).{|#13:ConfigureAwait(booleanVar)|}.GetAwaiter().GetResult();
				}

				[Fact]
				public async Task InvalidValue_TaskOfT_Triggers() {
					var booleanVar = true;
					var task = Task.FromResult(42);
					await task.{|#20:ConfigureAwait(false)|};
					await task.{|#21:ConfigureAwait(1 == 2)|};
					await task.{|#22:ConfigureAwait(1 == 1)|};
					await task.{|#23:ConfigureAwait(booleanVar)|};
				}

				[Fact]
				public async Task InvalidValue_ValueTask_Triggers() {
					var booleanVar = true;
					var valueTask = default(ValueTask);
					await valueTask.{|#30:ConfigureAwait(false)|};
					await valueTask.{|#31:ConfigureAwait(1 == 2)|};
					await valueTask.{|#32:ConfigureAwait(1 == 1)|};
					await valueTask.{|#33:ConfigureAwait(booleanVar)|};
				}

				[Fact]
				public async Task InvalidValue_ValueTaskOfT_Triggers() {
					var booleanVar = true;
					var valueTask = default(ValueTask<int>);
					await valueTask.{|#40:ConfigureAwait(false)|};
					await valueTask.{|#41:ConfigureAwait(1 == 2)|};
					await valueTask.{|#42:ConfigureAwait(1 == 1)|};
					await valueTask.{|#43:ConfigureAwait(booleanVar)|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("false", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(1).WithArguments("1 == 2", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(2).WithArguments("1 == 1", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(3).WithArguments("booleanVar", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),

			Verify.Diagnostic().WithLocation(10).WithArguments("false", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(11).WithArguments("1 == 2", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(12).WithArguments("1 == 1", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(13).WithArguments("booleanVar", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),

			Verify.Diagnostic().WithLocation(20).WithArguments("false", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(21).WithArguments("1 == 2", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(22).WithArguments("1 == 1", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(23).WithArguments("booleanVar", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),

			Verify.Diagnostic().WithLocation(30).WithArguments("false", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(31).WithArguments("1 == 2", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(32).WithArguments("1 == 1", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(33).WithArguments("booleanVar", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),

			Verify.Diagnostic().WithLocation(40).WithArguments("false", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(41).WithArguments("1 == 2", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(42).WithArguments("1 == 1", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
			Verify.Diagnostic().WithLocation(43).WithArguments("booleanVar", "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007."),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source, expected);
	}

#if NETCOREAPP

	[Fact]
	public async ValueTask V2_and_V3_NetCoreApp()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
				public async Task NonTestMethod_DoesNotTrigger() {
					await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.None);
				}
			}

			public class TestClass {
				[Fact]
				public async Task ValidValue_DoesNotTrigger() {
					await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext);
					await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext | ConfigureAwaitOptions.SuppressThrowing);
					await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.ContinueOnCapturedContext | ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding);
				}

				// Invalid values include:
				// - Any literal without ContinueOnCapturedContext
				// - Any variable

				[Fact]
				public async Task InvalidValue_InsideLambda_DoesNotTrigger() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					var t = Task.Run(async () => {
						await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.None);
						await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
						await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding);
						await Task.Delay(1).ConfigureAwait(enumVar);
					});
					await t;
				}

				[Fact]
				public async Task InvalidValue_InsideLocalFunction_DoesNotTrigger() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					async Task AssertEventStateAsync() {
						await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.None);
						await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
						await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding);
						await Task.Delay(1).ConfigureAwait(enumVar);
					}
				}

				[Fact]
				public async Task InvalidValue_TaskWithAwait_Triggers() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					await Task.Delay(1).{|#0:ConfigureAwait(ConfigureAwaitOptions.None)|};
					await Task.Delay(1).{|#1:ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing)|};
					await Task.Delay(1).{|#2:ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding)|};
					await Task.Delay(1).{|#3:ConfigureAwait(enumVar)|};
				}

				[Fact]
				public void InvalidValue_TaskWithoutAwait_Triggers() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					Task.Delay(1).{|#10:ConfigureAwait(ConfigureAwaitOptions.None)|}.GetAwaiter().GetResult();
					Task.Delay(1).{|#11:ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing)|}.GetAwaiter().GetResult();
					Task.Delay(1).{|#12:ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding)|}.GetAwaiter().GetResult();
					Task.Delay(1).{|#13:ConfigureAwait(enumVar)|}.GetAwaiter().GetResult();
				}

				[Fact]
				public async Task InvalidValue_TaskOfT_Triggers() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					var task = Task.FromResult(42);
					await task.{|#20:ConfigureAwait(ConfigureAwaitOptions.None)|};
					await task.{|#21:ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing)|};
					await task.{|#22:ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding)|};
					await task.{|#23:ConfigureAwait(enumVar)|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("ConfigureAwaitOptions.None", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(1).WithArguments("ConfigureAwaitOptions.SuppressThrowing", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(2).WithArguments("ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(3).WithArguments("enumVar", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),

			Verify.Diagnostic().WithLocation(10).WithArguments("ConfigureAwaitOptions.None", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(11).WithArguments("ConfigureAwaitOptions.SuppressThrowing", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(12).WithArguments("ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(13).WithArguments("enumVar", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),

			Verify.Diagnostic().WithLocation(20).WithArguments("ConfigureAwaitOptions.None", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(21).WithArguments("ConfigureAwaitOptions.SuppressThrowing", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(22).WithArguments("ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
			Verify.Diagnostic().WithLocation(23).WithArguments("enumVar", "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags."),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source, expected);
	}

#endif
}
