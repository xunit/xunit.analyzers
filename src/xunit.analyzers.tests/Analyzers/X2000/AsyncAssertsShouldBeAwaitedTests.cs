using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AsyncAssertsShouldBeAwaited>;

public class AsyncAssertsShouldBeAwaitedTests
{
	[Fact]
	public async Task UnawaitedNonAssertion_DoesNotTrigger()
	{
		var code = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void TestMethod() {
			        Task.Delay(1);
			    }
			}
			""";

		await Verify.VerifyAnalyzer(code);
	}

	readonly string codeTemplate = /* lang=c#-test */ """
		using System;
		using System.Collections.Generic;
		using System.ComponentModel;
		using System.Threading.Tasks;
		using Xunit;

		public class TestClass : INotifyPropertyChanged {{
		    public int Property {{ get; set; }}

		    public event PropertyChangedEventHandler? PropertyChanged;
		    public event EventHandler? SimpleEvent;
		    public event EventHandler<int>? SimpleIntEvent;

		    [Fact]
		    public async Task TestMethod() {{
		        {0}
		    }}
		}}

		public static class MyTaskExtensions {{
		    public static void ConsumeTask(this Task t) {{ }}
		}}
		""";

	public static TheoryData<string, string> AsyncAssertions = new()
	{
		/* lang=c#-test */ { "AllAsync", "Assert.AllAsync(default(IEnumerable<int>), i => Task.FromResult(true))" },
#if NETCOREAPP3_0_OR_GREATER
		/* lang=c#-test */ { "AllAsync", "Assert.AllAsync(default(IAsyncEnumerable<int>), i => Task.FromResult(true))" },
#endif
		/* lang=c#-test */ { "CollectionAsync", "Assert.CollectionAsync(default(IEnumerable<int>))" },
#if NETCOREAPP3_0_OR_GREATER
		/* lang=c#-test */ { "CollectionAsync", "Assert.CollectionAsync(default(IAsyncEnumerable<int>))" },
#endif
		/* lang=c#-test */ { "PropertyChangedAsync", "Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException())" },
		/* lang=c#-test */ { "RaisesAnyAsync", "Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException())" },
		/* lang=c#-test */ { "RaisesAnyAsync", "Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())" },
		/* lang=c#-test */ { "RaisesAsync", "Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())" },
		/* lang=c#-test */ { "ThrowsAnyAsync", "Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException())" },
		/* lang=c#-test */ { "ThrowsAsync", "Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException())" },
		/* lang=c#-test */ { "ThrowsAsync", "Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException())" },
		/* lang=c#-test */ { "ThrowsAsync", "Assert.ThrowsAsync<ArgumentException>(\"argName\", async () => throw new DivideByZeroException())" },
	};

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async Task AwaitedAssert_DoesNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"await {assertion};");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async Task AssertionWithConsumption_DoesNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"MyTaskExtensions.ConsumeTask({assertion});");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async Task AssertionWithConsumptionViaExtension_DoesNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"{assertion}.ConsumeTask();");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async Task AssertionWithStoredTask_DoesNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"var task = {assertion};");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async Task AssertionWithoutAwait_Triggers(
		string assertionName,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"{{|#0:{assertion}|}};");
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments(assertionName);

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async Task AssertionWithUnawaitedContinuation_Triggers(
		string assertionName,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"{{|#0:{assertion}|}}.ContinueWith(t => {{ }});");
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments(assertionName);

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code, expected);
	}
}
