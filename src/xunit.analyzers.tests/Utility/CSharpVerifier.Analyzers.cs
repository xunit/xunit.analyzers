using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

public partial class CSharpVerifier<TAnalyzer>
{
	// ----- Multi-version -----

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzer(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3(LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzer(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3(languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzer(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3(LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzer(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3(languageVersion, sources, diagnostics);
	}

	// ----- v2 -----

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	// ----- v3 -----

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3(LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3(languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3(LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}
}
