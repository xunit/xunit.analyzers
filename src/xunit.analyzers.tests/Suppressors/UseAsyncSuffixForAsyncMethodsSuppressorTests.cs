#if NETCOREAPP  // System.Collections.Immutable 1.6.0 conflicts with 6.0.0 in NetFx

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Suppressors.UseAsyncSuffixForAsyncMethodsSuppressor>;

public class UseAsyncSuffixForAsyncMethodsSuppressorTests
{
	[Fact]
	public async Task AcceptanceTest()
	{
		var code = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
			    public async Task {|#0:NonTestMethod|}() { }
			}

			public class TestClass {
			    [Fact]
			    public async Task {|#1:TestMethod|]() { }
			}
			""";
		var expected = new[]
		{
			new DiagnosticResult("VSTHRD200", DiagnosticSeverity.Warning).WithLocation(0),
			new DiagnosticResult("VSTHRD200", DiagnosticSeverity.Warning).WithLocation(1).WithIsSuppressed(true),
		};

		await Verify.VerifySuppressor(code, VsThreadingAnalyzers.VSTHRD200(), expected);
	}
}

#endif
