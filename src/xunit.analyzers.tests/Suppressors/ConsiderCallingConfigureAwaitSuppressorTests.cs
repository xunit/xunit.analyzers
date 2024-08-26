using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Suppressors.ConsiderCallingConfigureAwaitSuppressor>;

#if ROSLYN_LATEST
using System;
#endif

public sealed class ConsiderCallingConfigureAwaitSuppressorTests
{
	[Fact]
	public async Task NonTestMethod_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			using System.Threading.Tasks;

			public class NonTestClass {
			    public async Task NonTestMethod() {
			        await {|CA2007:Task.Delay(1)|};
			    }
			}
			""";

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA2007());
	}

	[Theory]
	[InlineData("Fact")]
	[InlineData("FactAttribute")]
	[InlineData("Theory")]
	[InlineData("TheoryAttribute")]
	public async Task StandardTestMethod_Suppresses(string attribute)
	{
		var code = string.Format(/* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {{
			    [{0}]
			    public async Task TestMethod() {{
			        await {{|#0:Task.Delay(1)|}};
			    }}
			}}
			""", attribute);
#if ROSLYN_LATEST
		var expected = Array.Empty<DiagnosticResult>();
#else
		var expected = DiagnosticResult.CompilerWarning("CA2007").WithLocation(0).WithIsSuppressed(true);
#endif

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA2007(), expected);
	}

	[Fact]
	public async Task CustomFactTestMethod_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			internal class CustomFactAttribute : FactAttribute { }

			public class TestClass {
			    [CustomFact]
			    public async Task TestMethod() {
			        await {|CA2007:Task.Delay(1)|};
			    }
			}
			""";

		await Verify.VerifySuppressor(code, CodeAnalysisNetAnalyzers.CA2007());
	}

	[Fact]
	public async Task CodeInsideFunctions_DoesNotSuppress()
	{
		var code = /* lang=c#-test */ """
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
			}
			""";

		await Verify.VerifySuppressor(LanguageVersion.CSharp7, code, CodeAnalysisNetAnalyzers.CA2007());
	}
}
