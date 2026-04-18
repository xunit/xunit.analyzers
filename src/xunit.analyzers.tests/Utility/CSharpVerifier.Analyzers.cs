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
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzer(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzer(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzer(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
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
	public static async Task VerifyAnalyzer(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(compilerDiagnostics, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3(compilerDiagnostics, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzer(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzer(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzer(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
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
	public static async Task VerifyAnalyzer(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(compilerDiagnostics, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3(compilerDiagnostics, languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(LanguageVersion.CSharp6, [source], diagnostics);
		await VerifyAnalyzerV3NonAot(LanguageVersion.CSharp6, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(compilerDiagnostics, languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3NonAot(compilerDiagnostics, languageVersion, [source], diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3NonAot(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerNonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(compilerDiagnostics, languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3NonAot(compilerDiagnostics, languageVersion, sources, diagnostics);
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
			VerifyAnalyzerV2(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);

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
			VerifyAnalyzerV2(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(compilerDiagnostics, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);

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
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.CompilerDiagnostics = compilerDiagnostics;
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
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, source, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, source, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(compilerDiagnostics, LanguageVersion.CSharp6, source, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(compilerDiagnostics, LanguageVersion.CSharp13, source, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, languageVersion, source, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, languageVersion, source, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
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
	public static async Task VerifyAnalyzerV3(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(compilerDiagnostics, languageVersion, source, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(compilerDiagnostics, languageVersion, source, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, sources, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(compilerDiagnostics, LanguageVersion.CSharp13, sources, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);
#endif
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
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
	public static async Task VerifyAnalyzerV3(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(compilerDiagnostics, languageVersion, sources, diagnostics);
#if NETCOREAPP
		await VerifyAnalyzerV3Aot(compilerDiagnostics, languageVersion, sources, diagnostics);
#endif
	}

#if NETCOREAPP

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using C# 13.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using C# 13.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(compilerDiagnostics, LanguageVersion.CSharp13, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(compilerDiagnostics, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using C# 13.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, LanguageVersion.CSharp13, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using C# 13.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(compilerDiagnostics, LanguageVersion.CSharp13, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		// We might be called from an API with a lower default C# version, so bump up to the minimum
		if (languageVersion < LanguageVersion.CSharp13)
			languageVersion = LanguageVersion.CSharp13;

		var testAot = new TestV3Aot(languageVersion);

		foreach (var source in sources)
			testAot.TestState.Sources.Add(source);

		testAot.CompilerDiagnostics = compilerDiagnostics;
		testAot.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		return testAot.RunAsync();
	}

#endif  // NETCOREAPP

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		CompilerDiagnostics compilerDiagnostics,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(compilerDiagnostics, LanguageVersion.CSharp6, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(compilerDiagnostics, languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		CompilerDiagnostics compilerDiagnostics,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(compilerDiagnostics, LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(CompilerDiagnostics.Errors, languageVersion, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="compilerDiagnostics">The level of compiler diagnostics to verify</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		CompilerDiagnostics compilerDiagnostics,
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.CompilerDiagnostics = compilerDiagnostics;
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}
}
