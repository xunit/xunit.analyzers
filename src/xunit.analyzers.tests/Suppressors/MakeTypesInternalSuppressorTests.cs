using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Suppressors.MakeTypesInternalSuppressor>;

#if ROSLYN_LATEST
using System;
#else
using Microsoft.CodeAnalysis;
#endif

public sealed class MakeTypesInternalSuppressorTests
{
	[Fact]
	public async Task NonTestClass_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			public class {|CA1515:NonTestClass|} {
			    public void NonTestMethod() { }
			}
			""";

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA1515());
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
		var code = string.Format(/* lang=c#-test */ """
			using Xunit;

			internal class CustomFactAttribute : FactAttribute {{ }}

			public class {{|#0:TestClass|}} {{
			    [{0}]
			    public void TestMethod() {{ }}
			}}
			""", attribute);
#if ROSLYN_LATEST
		var expected = Array.Empty<DiagnosticResult>();
#else
		var expected = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(true);
#endif

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA1515(), expected);
	}
}
