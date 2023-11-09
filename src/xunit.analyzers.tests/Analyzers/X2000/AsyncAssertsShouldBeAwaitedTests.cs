using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AsyncAssertsShouldBeAwaited>;

public class AsyncAssertsShouldBeAwaitedTests
{
	[Fact]
	public async void UnawaitedNonAssertionDoesNotTrigger()
	{
		var code = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        Task.Delay(1);
    }
}";

		await Verify.VerifyAnalyzer(code);
	}

	string codeTemplate = @"
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;

public class TestClass : INotifyPropertyChanged {{
    public int Property {{ get; set; }}

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? SimpleEvent;
    public event EventHandler<int>? SimpleIntEvent;

    [Fact]
    public async void TestMethod() {{
        {0}
    }}
}}

public static class MyTaskExtensions {{
    public static void ConsumeTask(this Task t) {{ }}
}}";

	public static TheoryData<string, string> AsyncAssertions = new()
	{
		{ "PropertyChangedAsync", "Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException())" },
		{ "RaisesAnyAsync", "Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException())" },
		{ "RaisesAnyAsync", "Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())" },
		{ "RaisesAsync", "Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())" },
		{ "ThrowsAnyAsync", "Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException())" },
		{ "ThrowsAsync", "Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException())" },
		{ "ThrowsAsync", "Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException())" },
		{ "ThrowsAsync", "Assert.ThrowsAsync<ArgumentException>(\"argName\", async () => throw new DivideByZeroException())" },
	};

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async void AwaitedAssertDoesNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"await {assertion};");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async void AssertionWithConsumptionNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"MyTaskExtensions.ConsumeTask({assertion});");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async void AssertionWithConsumptionViaExtensionNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"{assertion}.ConsumeTask();");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async void AssertionWithStoredTaskDoesNotTrigger(
		string _,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"var task = {assertion};");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async void AssertionWithoutAwaitTriggers(
		string assertionName,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"{assertion};");
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(16, 9, 16, 9 + assertion.Length)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments(assertionName);

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code, expected);
	}

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async void AssertionWithUnawaitedContinuationTriggers(
		string assertionName,
		string assertion)
	{
		var code = string.Format(CultureInfo.InvariantCulture, codeTemplate, $"{assertion}.ContinueWith(t => {{ }});");
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(16, 9, 16, 9 + assertion.Length)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments(assertionName);

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, code, expected);
	}
}
