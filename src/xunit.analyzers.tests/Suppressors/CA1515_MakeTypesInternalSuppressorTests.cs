using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Suppressors.MakeTypesInternalSuppressor>;

public sealed class CA1515_MakeTypesInternalSuppressorTests
{
	[Fact]
	public async Task V2_and_V3()
	{
		var code = /* lang=c#-test */ """
			using Xunit;

			public class {|CA1515:NonTestClass_DoesNotSuppress|} {
				public void NonTestMethod() { }
			}

			public class {|#0:Fact_TestClass_Suppresses|} {
				[Fact] public void TestMethod() { }
			}

			public class {|#1:Theory_TestClass_Suppresses|} {
				[Theory] public void TestMethod() { }
			}
			""";
		var expected = new[] {
			new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(true),
			new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(1).WithIsSuppressed(true),
		};

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA1515(), expected);
	}

	[Fact]
	public async Task V2_and_V3_NonAOT()
	{
		var code = /* lang=c#-test */ """
			using Xunit;

			internal class CustomFactAttribute : FactAttribute { }

			public class {|#0:CustomFact_TestClass_Suppresses|} {
				[CustomFact] public void TestMethod() { }
			}
			""";
		var expected = new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(true);

		await Verify.VerifySuppressorNonAot(code, CodeAnalysisNetAnalyzers.CA1515(), expected);
	}

	[Fact]
	public async Task V3_only()
	{
		var code = /* lang=c#-test */ """
			using Xunit;

			public class {|#0:CulturedFact_TestClass_Suppresses|} {
				[CulturedFact(new[] { "en-us" })] public void TestMethod() { }
			}

			public class {|#1:CulturedTheory_TestClass_Suppresses|} {
				[CulturedTheory(new[] { "en-us" })] public void TestMethod() { }
			}
			""";
		var expected = new[] {
			new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(0).WithIsSuppressed(true),
			new DiagnosticResult("CA1515", DiagnosticSeverity.Warning).WithLocation(1).WithIsSuppressed(true),
		};

		await Verify.VerifySuppressorV3(code, CodeAnalysisNetAnalyzers.CA1515(), expected);
	}
}
