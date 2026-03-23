using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Suppressors.ConsiderCallingConfigureAwaitSuppressor>;

public sealed class CA2007_ConsiderCallingConfigureAwaitSuppressorTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			class NonTestClass {
				async Task NonTestMethod() {
					await {|CA2007:Task.Delay(1)|};
				}
			}

			class TestClass {
				[Fact]
				async Task FactMethod() {
					await {|#0:Task.Delay(1)|};
				}

				[Theory]
				async Task TheoryMethod() {
					await {|#1:Task.Delay(1)|};
				}

				[Fact]
				void CodeInsideFunctions_DoesNotSuppress() {
					async Task InnerMethod1() { await {|CA2007:Task.Delay(1)|}; }
					async Task InnerMethod2() => await {|CA2007:Task.Delay(1)|};
					Func<Task> Lambda = async () => await {|CA2007:Task.Delay(1)|};
				}
			}
			""";
		var expected = new[] {
			DiagnosticResult.CompilerWarning("CA2007").WithLocation(0).WithIsSuppressed(true),
			DiagnosticResult.CompilerWarning("CA2007").WithLocation(1).WithIsSuppressed(true),
		};

		await Verify.VerifySuppressor(LanguageVersion.CSharp7, source, CodeAnalysisNetAnalyzers.CA2007(), expected);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			internal class CustomFactAttribute : FactAttribute { }

			class TestClass {
				[CustomFact]
				async Task CustomFactMethod() {
					await {|CA2007:Task.Delay(1)|};
				}
			}
			""";

		await Verify.VerifySuppressorNonAot(source, CodeAnalysisNetAnalyzers.CA2007());
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Threading.Tasks;
			using Xunit;

			class TestClass {
				[CulturedFact(new[] { "en-US" })]
				async Task CulturedFactMethod() {
					await {|#0:Task.Delay(1)|};
				}

				[CulturedTheory(new[] { "en-US" })]
				async Task CulturedTheoryMethod() {
					await {|#1:Task.Delay(1)|};
				}
			}
			""";
		var expected = new[] {
			DiagnosticResult.CompilerWarning("CA2007").WithLocation(0).WithIsSuppressed(true),
			DiagnosticResult.CompilerWarning("CA2007").WithLocation(1).WithIsSuppressed(true),
		};

		await Verify.VerifySuppressorV3(source, CodeAnalysisNetAnalyzers.CA2007(), expected);
	}
}
