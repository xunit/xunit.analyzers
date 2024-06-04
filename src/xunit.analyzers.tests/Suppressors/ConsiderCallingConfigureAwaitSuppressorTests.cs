#if NETCOREAPP

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Suppressors.ConsiderCallingConfigureAwaitSuppressor>;

public sealed class ConsiderCallingConfigureAwaitSuppressorTests
{
	[Fact]
	public async Task NonTestMethod_DoesNotSuppress()
	{
		var code = @"
using System.Threading.Tasks;

public class NonTestClass {
    public async Task NonTestMethod() {
        await {|CA2007:Task.Delay(1)|};
    }
}";

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA2007());
	}

	[Theory]
	[InlineData("Fact")]
	[InlineData("FactAttribute")]
	[InlineData("Theory")]
	[InlineData("TheoryAttribute")]
	public async Task StandardTestMethod_Suppresses(string attribute)
	{
		var code = @$"
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    [{attribute}]
    public async Task TestMethod() {{
        await Task.Delay(1);
    }}
}}";

		// Roslyn 3.11 still surfaces the diagnostic that has been suppressed
		var expected =
			new DiagnosticResult("CA2007", DiagnosticSeverity.Warning)
				.WithSpan(8, 15, 8, 28)
				.WithIsSuppressed(true);

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA2007(), expected);
	}

	[Fact]
	public async Task CustomFactTestMethod_DoesNotSuppress()
	{
		var code = @"
using System.Threading.Tasks;
using Xunit;

internal class CustomFactAttribute : FactAttribute { }

public class TestClass {
    [CustomFact]
    public async Task TestMethod() {
        await {|CA2007:Task.Delay(1)|};
    }
}";

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA2007());
	}

	[Fact]
	public async Task CodeInsideFunctions_DoesNotSuppress()
	{
		var code = @"
using System;
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        async Task InnerMethod1() { await {|CA2007:Task.Delay(1)|}; }
        async Task InnerMethod2() => await {|CA2007:Task.Delay(1)|};
        Func<Task> Lambda = async () => await {|CA2007:Task.Delay(1)|};
    }
}";

		await Verify.VerifySuppressor(LanguageVersion.CSharp7, code, CodeAnalysisNetAnalyzers.CA2007());
	}
}

#endif
