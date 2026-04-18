using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

public partial class CSharpVerifier<TAnalyzer>
{
	// ----- Multi-version -----

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerRunnerUtility(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerRunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtility(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtility(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtility(compilerDiagnostics, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerRunnerUtility(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerRunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtility(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(compilerDiagnostics, languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, languageVersion, sources, diagnostics);
	}

	// ----- v2 -----

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2RunnerUtility(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.CompilerDiagnostics = compilerDiagnostics;
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	// ----- v3 -----

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, [source], diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(compilerDiagnostics, LanguageVersion.CSharp13, [source], diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, languageVersion, [source], diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(compilerDiagnostics, languageVersion, [source], diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, sources, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(compilerDiagnostics, LanguageVersion.CSharp13, sources, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3RunnerUtility(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, languageVersion, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3RunnerUtilityAot(compilerDiagnostics, languageVersion, sources, diagnostics);
#endif
	}

#if NETCOREAPP

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 13.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityAot(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 13.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityAot(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityAot(compilerDiagnostics, LanguageVersion.CSharp13, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityAot(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityAot(compilerDiagnostics, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 13.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityAot(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 13.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityAot(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityAot(compilerDiagnostics, LanguageVersion.CSharp13, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerV3RunnerUtilityAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		// We might be called from an API with a lower default C# version, so bump up to the minimum
		if (languageVersion < LanguageVersion.CSharp13)
			languageVersion = LanguageVersion.CSharp13;

		var testAot = new TestV3RunnerUtilityAot(languageVersion);

		foreach (var source in sources)
			testAot.TestState.Sources.Add(source);

		testAot.CompilerDiagnostics = compilerDiagnostics;
		testAot.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		await testAot.RunAsync();
	}

#endif

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityNonAot(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityNonAot(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityNonAot(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityNonAot(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtilityNonAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtilityNonAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerV3RunnerUtilityNonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3RunnerUtility(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.CompilerDiagnostics = compilerDiagnostics;
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		await test.RunAsync();
	}
}
