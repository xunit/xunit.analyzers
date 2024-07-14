using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.UseCancellationToken>;

public class UseCancellationTokenTests
{
	[Fact]
	public async Task NoCancellationToken_DoesNotTrigger()
	{
		var source = @"
using System.Threading;
using Xunit;

class TestClass {
    [Fact]
    public void TestMethod() {
        Thread.Sleep(1);
    }
}";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Theory]
	[InlineData("TestContext.Current.CancellationToken")]
	[InlineData("new CancellationTokenSource().Token")]
	public async Task WithAnyCancellationToken_DoesNotTrigger(string token)
	{
		var source = @$"
using System.Threading;
using System.Threading.Tasks;
using Xunit;

class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        await Task.Delay(1, {token});
    }}
}}";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task WithoutCancellationToken_V2_DoesNotTrigger()
	{
		var source = @$"
using System.Threading;
using System.Threading.Tasks;
using Xunit;

class TestClass {{
    [Fact]
    public async Task TestMethod() {{
        await Task.Delay(1);
    }}
}}";

		await Verify.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async Task WithoutCancellationToken_WithoutDirectUpgrade_DoesNotTrigger()
	{
		var source = @"
using System.Threading;
using System.Threading.Tasks;
using Xunit;

class TestClass {
    [Fact]
    public void TestMethod() {
        FunctionWithOverload(42);
    }

    void FunctionWithOverload(int _) { }
    void FunctionWithOverload(CancellationToken _) { }
}";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Theory]
	[InlineData("FunctionWithDefaults()")]
	[InlineData("FunctionWithDefaults(42)")]
	[InlineData("FunctionWithDefaults(42, default)")]
	[InlineData("FunctionWithDefaults(42, default(CancellationToken))")]
	[InlineData("FunctionWithDefaults(cancellationToken: default)")]
	[InlineData("FunctionWithDefaults(cancellationToken: default(CancellationToken))")]
	[InlineData("FunctionWithOverload(42)")]
	[InlineData("FunctionWithOverload(42, default)")]
	[InlineData("FunctionWithOverload(42, default(CancellationToken))")]
	public async Task WithoutCancellationToken_V3_Triggers(string invocation)
	{
		var source = @$"
using System.Threading;
using System.Threading.Tasks;
using Xunit;

class TestClass {{
    [Fact]
    public void TestMethod() {{
        {invocation};
    }}

    void FunctionWithDefaults(int _1 = 2112, CancellationToken cancellationToken = default) {{ }}

    void FunctionWithOverload(int _) {{ }}
    void FunctionWithOverload(int _1, CancellationToken _2) {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(9, 9, 9, 9 + invocation.Length);

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source, expected);
	}
}
