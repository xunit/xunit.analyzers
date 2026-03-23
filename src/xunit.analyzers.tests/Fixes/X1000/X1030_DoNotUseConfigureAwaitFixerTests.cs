using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class X1030_DoNotUseConfigureAwaitFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task OnTask() {
					var booleanVar = true;

					await Task.Delay(1).[|ConfigureAwait(false)|];
					await Task.Delay(1).[|ConfigureAwait(1 == 1)|];
					await Task.Delay(1).[|ConfigureAwait(1 == 2)|];
					await Task.Delay(1).[|ConfigureAwait(booleanVar)|];

					Task.Delay(1).[|ConfigureAwait(false)|].GetAwaiter().GetResult();
					Task.Delay(1).[|ConfigureAwait(1 == 1)|].GetAwaiter().GetResult();
					Task.Delay(1).[|ConfigureAwait(1 == 2)|].GetAwaiter().GetResult();
					Task.Delay(1).[|ConfigureAwait(booleanVar)|].GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskOfT() {
					var booleanVar = true;

					await Task.FromResult(42).[|ConfigureAwait(false)|];
					await Task.FromResult(42).[|ConfigureAwait(1 == 1)|];
					await Task.FromResult(42).[|ConfigureAwait(1 == 2)|];
					await Task.FromResult(42).[|ConfigureAwait(booleanVar)|];

					Task.FromResult(42).[|ConfigureAwait(false)|].GetAwaiter().GetResult();
					Task.FromResult(42).[|ConfigureAwait(1 == 1)|].GetAwaiter().GetResult();
					Task.FromResult(42).[|ConfigureAwait(1 == 2)|].GetAwaiter().GetResult();
					Task.FromResult(42).[|ConfigureAwait(booleanVar)|].GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnValueTask() {
					var booleanVar = true;

					await default(ValueTask).[|ConfigureAwait(false)|];
					await default(ValueTask).[|ConfigureAwait(1 == 1)|];
					await default(ValueTask).[|ConfigureAwait(1 == 2)|];
					await default(ValueTask).[|ConfigureAwait(booleanVar)|];

					default(ValueTask).[|ConfigureAwait(false)|].GetAwaiter().GetResult();
					default(ValueTask).[|ConfigureAwait(1 == 1)|].GetAwaiter().GetResult();
					default(ValueTask).[|ConfigureAwait(1 == 2)|].GetAwaiter().GetResult();
					default(ValueTask).[|ConfigureAwait(booleanVar)|].GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnValueTaskOfT() {
					var booleanVar = true;

					await default(ValueTask<int>).[|ConfigureAwait(false)|];
					await default(ValueTask<int>).[|ConfigureAwait(1 == 1)|];
					await default(ValueTask<int>).[|ConfigureAwait(1 == 2)|];
					await default(ValueTask<int>).[|ConfigureAwait(booleanVar)|];

					default(ValueTask<int>).[|ConfigureAwait(false)|].GetAwaiter().GetResult();
					default(ValueTask<int>).[|ConfigureAwait(1 == 1)|].GetAwaiter().GetResult();
					default(ValueTask<int>).[|ConfigureAwait(1 == 2)|].GetAwaiter().GetResult();
					default(ValueTask<int>).[|ConfigureAwait(booleanVar)|].GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskVar() {
					var booleanVar = true;
					Task task = Task.FromResult(42);

					await task.[|ConfigureAwait(false)|];
					await task.[|ConfigureAwait(1 == 1)|];
					await task.[|ConfigureAwait(1 == 2)|];
					await task.[|ConfigureAwait(booleanVar)|];

					task.[|ConfigureAwait(false)|].GetAwaiter().GetResult();
					task.[|ConfigureAwait(1 == 1)|].GetAwaiter().GetResult();
					task.[|ConfigureAwait(1 == 2)|].GetAwaiter().GetResult();
					task.[|ConfigureAwait(booleanVar)|].GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskOfTVar() {
					var booleanVar = true;
					Task<int> task = Task.FromResult(42);

					await task.[|ConfigureAwait(false)|];
					await task.[|ConfigureAwait(1 == 1)|];
					await task.[|ConfigureAwait(1 == 2)|];
					await task.[|ConfigureAwait(booleanVar)|];

					task.[|ConfigureAwait(false)|].GetAwaiter().GetResult();
					task.[|ConfigureAwait(1 == 1)|].GetAwaiter().GetResult();
					task.[|ConfigureAwait(1 == 2)|].GetAwaiter().GetResult();
					task.[|ConfigureAwait(booleanVar)|].GetAwaiter().GetResult();
				}
			}
			""";
		var afterRemove = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task OnTask() {
					var booleanVar = true;

					await Task.Delay(1);
					await Task.Delay(1);
					await Task.Delay(1);
					await Task.Delay(1);

					Task.Delay(1).GetAwaiter().GetResult();
					Task.Delay(1).GetAwaiter().GetResult();
					Task.Delay(1).GetAwaiter().GetResult();
					Task.Delay(1).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskOfT() {
					var booleanVar = true;

					await Task.FromResult(42);
					await Task.FromResult(42);
					await Task.FromResult(42);
					await Task.FromResult(42);

					Task.FromResult(42).GetAwaiter().GetResult();
					Task.FromResult(42).GetAwaiter().GetResult();
					Task.FromResult(42).GetAwaiter().GetResult();
					Task.FromResult(42).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnValueTask() {
					var booleanVar = true;

					await default(ValueTask);
					await default(ValueTask);
					await default(ValueTask);
					await default(ValueTask);

					default(ValueTask).GetAwaiter().GetResult();
					default(ValueTask).GetAwaiter().GetResult();
					default(ValueTask).GetAwaiter().GetResult();
					default(ValueTask).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnValueTaskOfT() {
					var booleanVar = true;

					await default(ValueTask<int>);
					await default(ValueTask<int>);
					await default(ValueTask<int>);
					await default(ValueTask<int>);

					default(ValueTask<int>).GetAwaiter().GetResult();
					default(ValueTask<int>).GetAwaiter().GetResult();
					default(ValueTask<int>).GetAwaiter().GetResult();
					default(ValueTask<int>).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskVar() {
					var booleanVar = true;
					Task task = Task.FromResult(42);

					await task;
					await task;
					await task;
					await task;

					task.GetAwaiter().GetResult();
					task.GetAwaiter().GetResult();
					task.GetAwaiter().GetResult();
					task.GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskOfTVar() {
					var booleanVar = true;
					Task<int> task = Task.FromResult(42);

					await task;
					await task;
					await task;
					await task;

					task.GetAwaiter().GetResult();
					task.GetAwaiter().GetResult();
					task.GetAwaiter().GetResult();
					task.GetAwaiter().GetResult();
				}
			}
			""";
		var afterReplace = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task OnTask() {
					var booleanVar = true;

					await Task.Delay(1).ConfigureAwait(true);
					await Task.Delay(1).ConfigureAwait(true);
					await Task.Delay(1).ConfigureAwait(true);
					await Task.Delay(1).ConfigureAwait(true);

					Task.Delay(1).ConfigureAwait(true).GetAwaiter().GetResult();
					Task.Delay(1).ConfigureAwait(true).GetAwaiter().GetResult();
					Task.Delay(1).ConfigureAwait(true).GetAwaiter().GetResult();
					Task.Delay(1).ConfigureAwait(true).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskOfT() {
					var booleanVar = true;

					await Task.FromResult(42).ConfigureAwait(true);
					await Task.FromResult(42).ConfigureAwait(true);
					await Task.FromResult(42).ConfigureAwait(true);
					await Task.FromResult(42).ConfigureAwait(true);

					Task.FromResult(42).ConfigureAwait(true).GetAwaiter().GetResult();
					Task.FromResult(42).ConfigureAwait(true).GetAwaiter().GetResult();
					Task.FromResult(42).ConfigureAwait(true).GetAwaiter().GetResult();
					Task.FromResult(42).ConfigureAwait(true).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnValueTask() {
					var booleanVar = true;

					await default(ValueTask).ConfigureAwait(true);
					await default(ValueTask).ConfigureAwait(true);
					await default(ValueTask).ConfigureAwait(true);
					await default(ValueTask).ConfigureAwait(true);

					default(ValueTask).ConfigureAwait(true).GetAwaiter().GetResult();
					default(ValueTask).ConfigureAwait(true).GetAwaiter().GetResult();
					default(ValueTask).ConfigureAwait(true).GetAwaiter().GetResult();
					default(ValueTask).ConfigureAwait(true).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnValueTaskOfT() {
					var booleanVar = true;

					await default(ValueTask<int>).ConfigureAwait(true);
					await default(ValueTask<int>).ConfigureAwait(true);
					await default(ValueTask<int>).ConfigureAwait(true);
					await default(ValueTask<int>).ConfigureAwait(true);

					default(ValueTask<int>).ConfigureAwait(true).GetAwaiter().GetResult();
					default(ValueTask<int>).ConfigureAwait(true).GetAwaiter().GetResult();
					default(ValueTask<int>).ConfigureAwait(true).GetAwaiter().GetResult();
					default(ValueTask<int>).ConfigureAwait(true).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskVar() {
					var booleanVar = true;
					Task task = Task.FromResult(42);

					await task.ConfigureAwait(true);
					await task.ConfigureAwait(true);
					await task.ConfigureAwait(true);
					await task.ConfigureAwait(true);

					task.ConfigureAwait(true).GetAwaiter().GetResult();
					task.ConfigureAwait(true).GetAwaiter().GetResult();
					task.ConfigureAwait(true).GetAwaiter().GetResult();
					task.ConfigureAwait(true).GetAwaiter().GetResult();
				}

				[Fact]
				public async Task OnTaskOfTVar() {
					var booleanVar = true;
					Task<int> task = Task.FromResult(42);

					await task.ConfigureAwait(true);
					await task.ConfigureAwait(true);
					await task.ConfigureAwait(true);
					await task.ConfigureAwait(true);

					task.ConfigureAwait(true).GetAwaiter().GetResult();
					task.ConfigureAwait(true).GetAwaiter().GetResult();
					task.ConfigureAwait(true).GetAwaiter().GetResult();
					task.ConfigureAwait(true).GetAwaiter().GetResult();
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, afterRemove, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
		await Verify.VerifyCodeFixFixAll(before, afterReplace, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
	}

#if NETCOREAPP

	[Fact]
	public async ValueTask V2_and_V3_ConfigureAwaitOptions()
	{
		var before = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task OnTask() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;

					await Task.Delay(1).[|ConfigureAwait(enumVar)|];
					await Task.Delay(1).[|ConfigureAwait(ConfigureAwaitOptions.None)|];
					await Task.Delay(1).[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing)|];
					await Task.Delay(1).[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding)|];
				}

				[Fact]
				public async Task OnTaskOfT() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;

					await Task.FromResult(42).[|ConfigureAwait(enumVar)|];
					await Task.FromResult(42).[|ConfigureAwait(ConfigureAwaitOptions.None)|];
					await Task.FromResult(42).[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing)|];
					await Task.FromResult(42).[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding)|];
				}

				[Fact]
				public async Task OnTaskVar() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					var task = Task.Delay(1);

					await task.[|ConfigureAwait(enumVar)|];
					await task.[|ConfigureAwait(ConfigureAwaitOptions.None)|];
					await task.[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing)|];
					await task.[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding)|];
				}

				[Fact]
				public async Task OnTaskOfTVar() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					var task = Task.FromResult(42);

					await task.[|ConfigureAwait(enumVar)|];
					await task.[|ConfigureAwait(ConfigureAwaitOptions.None)|];
					await task.[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing)|];
					await task.[|ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				[Fact]
				public async Task OnTask() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;

					await Task.Delay(1).ConfigureAwait(enumVar | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.None | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.ContinueOnCapturedContext);
				}

				[Fact]
				public async Task OnTaskOfT() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;

					await Task.FromResult(42).ConfigureAwait(enumVar | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await Task.FromResult(42).ConfigureAwait(ConfigureAwaitOptions.None | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await Task.FromResult(42).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await Task.FromResult(42).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.ContinueOnCapturedContext);
				}

				[Fact]
				public async Task OnTaskVar() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					var task = Task.Delay(1);

					await task.ConfigureAwait(enumVar | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await task.ConfigureAwait(ConfigureAwaitOptions.None | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.ContinueOnCapturedContext);
				}

				[Fact]
				public async Task OnTaskOfTVar() {
					var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
					var task = Task.FromResult(42);

					await task.ConfigureAwait(enumVar | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await task.ConfigureAwait(ConfigureAwaitOptions.None | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
					await task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.ContinueOnCapturedContext);
				}
			}
			""";

		await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
	}

#endif  // NETCOREAPP
}
