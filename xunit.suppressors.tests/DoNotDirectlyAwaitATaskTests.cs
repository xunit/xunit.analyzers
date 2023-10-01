using Microsoft.CodeAnalysis.Testing;
using Test = AnalyzerTesting.CSharp.Extensions.CSharpDiagnosticSuppressorTest<
	Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.DoNotDirectlyAwaitATaskAnalyzer,
	Xunit.Analyzers.Suppressors.DoNotDirectlyAwaitATaskSuppressor,
	Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace xunit.suppressors.tests;

public class DoNotDirectlyAwaitATaskTests
{
	[Fact]
	public async Task IsNotSuppressedWhenNotInTestContext()
	{
		const string source =
			"""
			using System.Threading.Tasks;
			class C
			{
				async Task M()
				{
					await {|#0:Task.Delay(1)|};
				}
			}
			""";

		var expected = new[]
		{
			DiagnosticResult.CompilerWarning("CA2007").WithLocation(0)
		};

		await new Test()
			.AddSources(source)
			.AddExpectedDiagnostics(expected)
			.RunAsync();
	}

	[Fact]
	public async Task IsSuppressedWhenInTestContext()
	{
		const string source =
			"""
			using System.Threading.Tasks;
			using Xunit;
			class C
			{
				[Fact]
				async Task M()
				{
					await {|#0:Task.Delay(1)|};
				}
			}
			""";

		var expected = new[]
		{
			DiagnosticResult.CompilerWarning("CA2007").WithLocation(0).WithIsSuppressed(true)
		};

		await new Test()
			.AddPackages(new PackageIdentity("xunit.core", "2.4.2"))
			.AddSources(source)
			.AddExpectedDiagnostics(expected)
			.RunAsync();
	}
}
