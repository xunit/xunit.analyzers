#if NETCOREAPP

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Suppressors.MakeTypesInternalSuppressor>;

public sealed class MakeTypesInternalSuppressorTests
{
	[Fact]
	public async Task NonTestClass_DoesNotSuppress()
	{
		var code = @"
public class NonTestClass {
    public void NonTestMethod() { }
}";

		var expected =
			new DiagnosticResult("CA1515", DiagnosticSeverity.Warning)
				.WithSpan(2, 14, 2, 26)
				.WithIsSuppressed(false);

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA1515(), expected);
	}

	[Theory]
	[InlineData("Fact")]
	[InlineData("FactAttribute")]
	[InlineData("Theory")]
	[InlineData("TheoryAttribute")]
	[InlineData("CustomFact")]
	[InlineData("CustomFactAttribute")]
	public async Task TestClass_Suppresses(string attribute)
	{
		var code = @$"
using Xunit;

internal class CustomFactAttribute : FactAttribute {{ }}

public class TestClass {{
    [{attribute}]
    public void TestMethod() {{ }}
}}";

		// Roslyn 3.11 still surfaces the diagnostic that has been suppressed
		var expected =
			new DiagnosticResult("CA1515", DiagnosticSeverity.Warning)
				.WithSpan(6, 14, 6, 23)
				.WithIsSuppressed(true);

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA1515(), expected);
	}
}

#endif
