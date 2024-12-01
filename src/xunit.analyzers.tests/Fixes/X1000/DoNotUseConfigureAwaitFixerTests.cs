using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class DoNotUseConfigureAwaitFixerTests
{
	public class ConfigureAwait_Boolean
	{
		public static TheoryData<string> InvalidValues =
		[
			"false",       // Literal false
			"1 == 2",      // Logical false (we don't compute)
			"1 == 1",      // Logical true (we don't compute)
			"booleanVar",  // Reference value (we don't do lookup)
		];

		public class RemoveConfigureAwait
		{
			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task Task_Async(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							await Task.Delay(1).[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							await Task.Delay(1);
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task Task_NonAsync(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public void TestMethod() {{
							var booleanVar = true;
							Task.Delay(1).[|ConfigureAwait({0})|].GetAwaiter().GetResult();
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public void TestMethod() {
							var booleanVar = true;
							Task.Delay(1).GetAwaiter().GetResult();
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task TaskOfT(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							var task = Task.FromResult(42);
							await task.[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							var task = Task.FromResult(42);
							await task;
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task ValueTask(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							var valueTask = default(ValueTask);
							await valueTask.[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							var valueTask = default(ValueTask);
							await valueTask;
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task ValueTaskOfT(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							var valueTask = default(ValueTask<object>);
							await valueTask.[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							var valueTask = default(ValueTask<object>);
							await valueTask;
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_RemoveConfigureAwait);
			}
		}

		public class ReplaceConfigureAwait
		{
			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task Task_Async(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							await Task.Delay(1).[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							await Task.Delay(1).ConfigureAwait(true);
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task Task_NonAsync(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public void TestMethod() {{
							var booleanVar = true;
							Task.Delay(1).[|ConfigureAwait({0})|].GetAwaiter().GetResult();
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public void TestMethod() {
							var booleanVar = true;
							Task.Delay(1).ConfigureAwait(true).GetAwaiter().GetResult();
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task TaskOfT(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							var task = Task.FromResult(42);
							await task.[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							var task = Task.FromResult(42);
							await task.ConfigureAwait(true);
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task ValueTask(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							var valueTask = default(ValueTask);
							await valueTask.[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							var valueTask = default(ValueTask);
							await valueTask.ConfigureAwait(true);
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
			}

			[Theory]
			[MemberData(nameof(InvalidValues), MemberType = typeof(ConfigureAwait_Boolean))]
			public async Task ValueTaskOfT(string argumentValue)
			{
				var before = string.Format(/* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {{
						[Fact]
						public async Task TestMethod() {{
							var booleanVar = true;
							var valueTask = default(ValueTask<object>);
							await valueTask.[|ConfigureAwait({0})|];
						}}
					}}
					""", argumentValue);
				var after = /* lang=c#-test */ """
					using System.Threading.Tasks;
					using Xunit;

					public class TestClass {
						[Fact]
						public async Task TestMethod() {
							var booleanVar = true;
							var valueTask = default(ValueTask<object>);
							await valueTask.ConfigureAwait(true);
						}
					}
					""";

				await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
			}
		}
	}

#if NETCOREAPP

	public class ConfigureAwait_ConfigureAwaitOptions
	{
		public static TheoryData<string> InvalidValues =
		[
			// Literal values
			/* lang=c#-test */ "ConfigureAwaitOptions.None",
			/* lang=c#-test */ "ConfigureAwaitOptions.SuppressThrowing",
			/* lang=c#-test */ "ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.SuppressThrowing",

			// Reference values (we don't do lookup)
			/* lang=c#-test */ "enumVar",
		];

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task Task_Async(string argumentValue)
		{
			var before = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
					[Fact]
					public async Task TestMethod() {{
						var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
						await Task.Delay(1).[|ConfigureAwait({0})|];
					}}
				}}
				""", argumentValue);
			var after = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
					[Fact]
					public async Task TestMethod() {{
						var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
						await Task.Delay(1).ConfigureAwait({0} | ConfigureAwaitOptions.ContinueOnCapturedContext);
					}}
				}}
				""", argumentValue);

			await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task Task_NonAsync(string argumentValue)
		{
			var before = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
					[Fact]
					public void TestMethod() {{
						var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
						Task.Delay(1).[|ConfigureAwait({0})|].GetAwaiter().GetResult();
					}}
				}}
				""", argumentValue);
			var after = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
					[Fact]
					public void TestMethod() {{
						var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
						Task.Delay(1).ConfigureAwait({0} | ConfigureAwaitOptions.ContinueOnCapturedContext).GetAwaiter().GetResult();
					}}
				}}
				""", argumentValue);

			await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task TaskOfT(string argumentValue)
		{
			var before = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
					[Fact]
					public async Task TestMethod() {{
						var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
						var task = Task.FromResult(42);
						await task.[|ConfigureAwait({0})|];
					}}
				}}
				""", argumentValue);
			var after = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
					[Fact]
					public async Task TestMethod() {{
						var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
						var task = Task.FromResult(42);
						await task.ConfigureAwait({0} | ConfigureAwaitOptions.ContinueOnCapturedContext);
					}}
				}}
				""", argumentValue);

			await Verify.VerifyCodeFix(before, after, DoNotUseConfigureAwaitFixer.Key_ReplaceArgumentValue);
		}
	}

#endif
}
