using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseConfigureAwait>;

public class DoNotUseConfigureAwaitTests
{
	[Fact]
	public async void NoCall_DoesNotTrigger()
	{
		var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        await Task.Delay(1);
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	public class ConfigureAwait_Boolean
	{
		[Fact]
		public async void NonTestMethod_DoesNotTrigger()
		{
			var source = @"
using System.Threading.Tasks;
using Xunit;

public class NonTestClass {
    public async Task NonTestMethod() {
        await Task.Delay(1).ConfigureAwait(false);
    }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void True_DoesNotTrigger()
		{
			var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        await Task.Delay(1).ConfigureAwait(true);
    }
}";

			await Verify.VerifyAnalyzer(source);
		}

		public static TheoryData<string> InvalidValues = new()
		{
			"false",       // Literal false
			"1 == 2",      // Logical false (we don't compute)
			"1 == 1",      // Logical true (we don't compute)
			"booleanVar",  // Reference value (we don't do lookup)
		};

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_InsideLambda_DoesNotTrigger(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
        var t = Task.Run(async () => {{
            await Task.Delay(1).ConfigureAwait({argumentValue});
        }});
        await t;
    }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_InsideLocalFunction_DoesNotTrigger(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
        
        async Task AssertEventStateAsync()
        {{
            await Task.Delay(1).ConfigureAwait({argumentValue});
        }}
    }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_TaskWithAwait_Triggers(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
        await Task.Delay(1).ConfigureAwait({argumentValue});
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(9, 29, 9, 45 + argumentValue.Length)
					.WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_TaskWithoutAwait_Triggers(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var booleanVar = true;
        Task.Delay(1).ConfigureAwait({argumentValue}).GetAwaiter().GetResult();
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(9, 23, 9, 39 + argumentValue.Length)
					.WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_TaskOfT_Triggers(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
        var task = Task.FromResult(42);
        await task.ConfigureAwait({argumentValue});
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(10, 20, 10, 36 + argumentValue.Length)
					.WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_ValueTask_Triggers(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
        var valueTask = default(ValueTask);
        await valueTask.ConfigureAwait({argumentValue});
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(10, 25, 10, 41 + argumentValue.Length)
					.WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_ValueTaskOfT_Triggers(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var booleanVar = true;
        var valueTask = default(ValueTask<int>);
        await valueTask.ConfigureAwait({argumentValue});
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(10, 25, 10, 41 + argumentValue.Length)
					.WithArguments(argumentValue, "Omit ConfigureAwait, or use ConfigureAwait(true) to avoid CA2007.");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

#if NETCOREAPP

	public class ConfigureAwait_ConfigureAwaitOptions
	{
		[Fact]
		public async void NonTestMethod_DoesNotTrigger()
		{
			var source = @"
using System.Threading.Tasks;
using Xunit;

public class NonTestClass {
    public async Task NonTestMethod() {
        await Task.Delay(1).ConfigureAwait(ConfigureAwaitOptions.None);
    }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("ConfigureAwaitOptions.ContinueOnCapturedContext")]
		[InlineData("ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext")]
		[InlineData("ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext")]
		public async void ValidValue_DoesNotTrigger(string enumValue)
		{
			var source = $@"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        await Task.Delay(1).ConfigureAwait({enumValue});
    }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		public static TheoryData<string> InvalidValues = new()
		{
			// Literal values
			"ConfigureAwaitOptions.None",
			"ConfigureAwaitOptions.SuppressThrowing",
			"ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.SuppressThrowing",
			// Reference values (we don't do lookup)
			"enumVar",
		};

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_InsideLambda_DoesNotTrigger(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
        var t = Task.Run(async () => {{
            await Task.Delay(1).ConfigureAwait({argumentValue});
        }});
        await t;
    }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_InsideLocalFunction_DoesNotTrigger(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
        
        async Task AssertEventStateAsync()
        {{
            await Task.Delay(1).ConfigureAwait({argumentValue});
        }}
    }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_TaskWithAwait_Triggers(string enumValue)
		{
			var source = $@"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
        await Task.Delay(1).ConfigureAwait({enumValue});
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(9, 29, 9, 45 + enumValue.Length)
					.WithArguments(enumValue, "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_TaskWithoutAwait_Triggers(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
        Task.Delay(1).ConfigureAwait({argumentValue}).GetAwaiter().GetResult();
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(9, 23, 9, 39 + argumentValue.Length)
					.WithArguments(argumentValue, "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags.");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(InvalidValues))]
		public async void InvalidValue_TaskOfT_Triggers(string argumentValue)
		{
			var source = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        var enumVar = ConfigureAwaitOptions.ContinueOnCapturedContext;
        var task = Task.FromResult(42);
        await task.ConfigureAwait({argumentValue});
    }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(10, 20, 10, 36 + argumentValue.Length)
					.WithArguments(argumentValue, "Ensure ConfigureAwaitOptions.ContinueOnCapturedContext in the flags.");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

#endif
}
