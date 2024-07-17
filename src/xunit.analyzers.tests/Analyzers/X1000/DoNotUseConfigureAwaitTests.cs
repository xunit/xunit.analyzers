using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class DoNotUseConfigureAwaitTests
{
	[Fact]
	public async Task NoCall_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
			    [Fact]
			    public async Task TestMethod() {
			        await Task.Delay(1);
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	public class ConfigureAwait_Boolean
	{
		[Fact]
		public async Task NonTestMethod_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class NonTestClass {
				    public async Task NonTestMethod() {
				        await Task.Delay(1).ConfigureAwait(false);
				    }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task True_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {
				    [Fact]
				    public async Task TestMethod() {
				        await Task.Delay(1).ConfigureAwait(true);
				    }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		public static TheoryData<string> InvalidValues =
		[
			"false",       // Literal false
			"1 == 2",      // Logical false (we don't compute)
			"1 == 1",      // Logical true (we don't compute)
			"booleanVar",  // Reference value (we don't do lookup)
		];

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_InsideLambda_DoesNotTrigger(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var booleanVar = true;
				        var t = Task.Run(async () => {{
				            await Task.Delay(1).ConfigureAwait({0});
				        }});
				        await t;
				    }}
				}}
				""", argumentValue);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_InsideLocalFunction_DoesNotTrigger(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var booleanVar = true;
				        async Task AssertEventStateAsync() {{
				            await Task.Delay(1).ConfigureAwait({0});
				        }}
				    }}
				}}
				""", argumentValue);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_TaskWithAwait_Triggers(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var booleanVar = true;
				        await Task.Delay(1).{{|#0:ConfigureAwait({0})|}};
				    }}
				}}
				""", argumentValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_TaskWithoutAwait_Triggers(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public void TestMethod() {{
				        var booleanVar = true;
				        Task.Delay(1).{{|#0:ConfigureAwait({0})|}}.GetAwaiter().GetResult();
				    }}
				}}
				""", argumentValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_TaskOfT_Triggers(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var booleanVar = true;
				        var task = Task.FromResult(42);
				        await task.{{|#0:ConfigureAwait({0})|}};
				    }}
				}}
				""", argumentValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_ValueTask_Triggers(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var booleanVar = true;
				        var valueTask = default(ValueTask);
				        await valueTask.{{|#0:ConfigureAwait({0})|}};
				    }}
				}}
				""", argumentValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_ValueTaskOfT_Triggers(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var booleanVar = true;
				        var valueTask = default(ValueTask<int>);
				        await valueTask.{{|#0:ConfigureAwait({0})|}};
				    }}
				}}
				""", argumentValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

#if NETCOREAPP

	public class ConfigureAwait_ConfigureAwaitOptions
	{
		[Fact]
		public async Task NonTestMethod_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class NonTestClass {
				    public async Task NonTestMethod() {
				        await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.None);
				    }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("ConfigureAwaitOptions.ContinueOnCapturedContext")]
		[InlineData("ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext")]
		[InlineData("ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext")]
		public async Task ValidValue_DoesNotTrigger(string enumValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        await Task.Delay(1).ConfigureAwait({0});
				    }}
				}}
				""", enumValue);

			await Verify.VerifyAnalyzer(source);
		}

		public static TheoryData<string> InvalidValues =
		[
			// Literal values
			"ConfigureAwaitOptions.None",
			"ConfigureAwaitOptions.SuppressThrowing",
			"ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.SuppressThrowing",
			// Reference values (we don't do lookup)
			"enumVar",
		];

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_InsideLambda_DoesNotTrigger(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
				        var t = Task.Run(async () => {{
				            await Task.Delay(1).ConfigureAwait({0});
				        }});
				        await t;
				    }}
				}}
				""", argumentValue);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_InsideLocalFunction_DoesNotTrigger(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
				        async Task AssertEventStateAsync() {{
				            await Task.Delay(1).ConfigureAwait({0});
				        }}
				    }}
				}}
				""", argumentValue);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_TaskWithAwait_Triggers(string enumValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
				        await Task.Delay(1).{{|#0:ConfigureAwait({0})|}};
				    }}
				}}
				""", enumValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(enumValue, "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_TaskWithoutAwait_Triggers(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public void TestMethod() {{
				        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
				        Task.Delay(1).{{|#0:ConfigureAwait({0})|}}.GetAwaiter().GetResult();
				    }}
				}}
				""", argumentValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(argumentValue, "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async Task InvalidValue_TaskOfT_Triggers(string argumentValue)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System.Threading.Tasks;
				using Xunit;

				public class TestClass {{
				    [Fact]
				    public async Task TestMethod() {{
				        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
				        var task = Task.FromResult(42);
				        await task.{{|#0:ConfigureAwait({0})|}};
				    }}
				}}
				""", argumentValue);
			var expected = Verify.Diagnostic().WithLocation(0).WithArguments(argumentValue, "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags.");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

#endif
}
