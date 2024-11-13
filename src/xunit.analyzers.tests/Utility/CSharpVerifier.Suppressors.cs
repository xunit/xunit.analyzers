using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

public partial class CSharpVerifier<TAnalyzer>
{
	// ----- Multi-version -----

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressor(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);
		await VerifySuppressorV3(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressor(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(languageVersion, [source], [suppressedAnalyzer], diagnostics);
		await VerifySuppressorV3(languageVersion, [source], [suppressedAnalyzer], diagnostics);
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressor(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(languageVersion, sources, suppressedAnalyzers, diagnostics);
		await VerifySuppressorV3(languageVersion, sources, suppressedAnalyzers, diagnostics);
	}

	// ----- v2 -----

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV2(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV2(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV2(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV2(languageVersion, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV2(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2(languageVersion);

		foreach (var suppressedAnalyzer in suppressedAnalyzers)
			test.AddDiagnosticAnalyzer(suppressedAnalyzer);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		test.TestState.OutputKind = OutputKind.ConsoleApplication;
		test.TestState.Sources.Add("internal class Program { public static void Main() { } }");
		return test.RunAsync();
	}

	// ----- v3 -----

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV3(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV3(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV3(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV3(languageVersion, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV3(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3(languageVersion);

		foreach (var suppressedAnalyzer in suppressedAnalyzers)
			test.AddDiagnosticAnalyzer(suppressedAnalyzer);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		test.TestState.OutputKind = OutputKind.ConsoleApplication;
		test.TestState.Sources.Add("internal class Program { public static void Main() { } }");
		return test.RunAsync();
	}
}
