using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AsyncAssertsShouldBeAwaited>;

public class AsyncAssertsShouldBeAwaitedFixerTests
{
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
    public void TestMethod() {{
        {0}
    }}
}}

public static class MyTaskExtensions {{
    public static void ConsumeTask(this Task t) {{ }}
}}";

	public static TheoryData<string> AsyncAssertions = new()
	{
		"Assert.PropertyChangedAsync(this, nameof(Property), async () => throw new DivideByZeroException())",
		"Assert.RaisesAnyAsync(eh => SimpleEvent += eh, eh => SimpleEvent -= eh, async () => throw new DivideByZeroException())",
		"Assert.RaisesAnyAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())",
		"Assert.RaisesAsync<int>(eh => SimpleIntEvent += eh, eh => SimpleIntEvent -= eh, async () => throw new DivideByZeroException())",
		"Assert.ThrowsAnyAsync<Exception>(async () => throw new DivideByZeroException())",
		"Assert.ThrowsAsync(typeof(DivideByZeroException), async () => throw new DivideByZeroException())",
		"Assert.ThrowsAsync<DivideByZeroException>(async () => throw new DivideByZeroException())",
		"Assert.ThrowsAsync<ArgumentException>(\"argName\", async () => throw new DivideByZeroException())",
	};

	[Theory]
	[MemberData(nameof(AsyncAssertions))]
	public async void AddsAsyncAndAwait(string assertion)
	{
		var before = string.Format(codeTemplate, $"[|{assertion}|];");
		var after = string.Format(codeTemplate, $"await {assertion};").Replace("public void TestMethod", "public async Task TestMethod");

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, AsyncAssertsShouldBeAwaitedFixer.Key_AddAwait);
	}
}
