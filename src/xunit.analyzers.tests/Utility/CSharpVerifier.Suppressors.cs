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
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
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
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
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
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifySuppressor(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(languageVersion, sources, suppressedAnalyzers, diagnostics);
		await VerifySuppressorV3(languageVersion, sources, suppressedAnalyzers, diagnostics);
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifySuppressorAot(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);
		await VerifySuppressorV3Aot(LanguageVersion.CSharp13, [source], [suppressedAnalyzer], diagnostics);
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifySuppressorAot(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(languageVersion, [source], [suppressedAnalyzer], diagnostics);
		await VerifySuppressorV3Aot(languageVersion, [source], [suppressedAnalyzer], diagnostics);
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifySuppressorAot(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(languageVersion, sources, suppressedAnalyzers, diagnostics);
		await VerifySuppressorV3Aot(languageVersion, sources, suppressedAnalyzers, diagnostics);
	}

#endif // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressorNonAot(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);
		await VerifySuppressorV3NonAot(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressorNonAot(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(languageVersion, [source], [suppressedAnalyzer], diagnostics);
		await VerifySuppressorV3NonAot(languageVersion, [source], [suppressedAnalyzer], diagnostics);
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressorNonAot(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV2(languageVersion, sources, suppressedAnalyzers, diagnostics);
		await VerifySuppressorV3NonAot(languageVersion, sources, suppressedAnalyzers, diagnostics);
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress a compiler warning. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#. Sets CompilerDiagnostics
	/// to Warnings so that compiler warnings (like CS8618) are included in the analysis.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCompilerWarningSuppressor(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCompilerWarningSuppressorV2(languageVersion, sources, diagnostics);
		await VerifyCompilerWarningSuppressorV3(languageVersion, sources, diagnostics);
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress a compiler warning. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#. Sets CompilerDiagnostics
	/// to Warnings so that compiler warnings (like CS8618) are included in the analysis.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCompilerWarningSuppressorAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCompilerWarningSuppressorV2(languageVersion, sources, diagnostics);
		await VerifyCompilerWarningSuppressorV3Aot(languageVersion, sources, diagnostics);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress a compiler warning. Runs against
	/// xUnit.net v2 and v3, using the provided version of C#. Sets CompilerDiagnostics
	/// to Warnings so that compiler warnings (like CS8618) are included in the analysis.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifyCompilerWarningSuppressorNonAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCompilerWarningSuppressorV2(languageVersion, sources, diagnostics);
		await VerifyCompilerWarningSuppressorV3NonAot(languageVersion, sources, diagnostics);
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

	/// <summary>
	/// Verify that an analyzer was used to suppress a compiler warning. Runs against
	/// xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifyCompilerWarningSuppressorV2(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.CompilerDiagnostics = CompilerDiagnostics.Warnings;
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		test.TestState.OutputKind = OutputKind.ConsoleApplication;
		test.TestState.Sources.Add("internal class Program { public static void Main() { } }");
		test.SolutionTransforms.Add((solution, projectId) =>
		{
			var project = solution.GetProject(projectId)!;
			var compilationOptions = project.CompilationOptions!;
			compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
				compilationOptions.SpecificDiagnosticOptions.SetItem("CS1701", ReportDiagnostic.Suppress)
			);
			return solution.WithProjectCompilationOptions(projectId, compilationOptions);
		});
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
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifySuppressorV3(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV3NonAot(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifySuppressorV3Aot(LanguageVersion.CSharp13, [source], [suppressedAnalyzer], diagnostics);
#endif
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifySuppressorV3(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV3NonAot(languageVersion, [source], [suppressedAnalyzer], diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifySuppressorV3Aot(languageVersion, [source], [suppressedAnalyzer], diagnostics);
#endif
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifySuppressorV3(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		await VerifySuppressorV3NonAot(languageVersion, sources, suppressedAnalyzers, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifySuppressorV3Aot(languageVersion, sources, suppressedAnalyzers, diagnostics);
#endif
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v3, using C# 13.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV3Aot(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV3Aot(LanguageVersion.CSharp13, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV3Aot(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV3Aot(languageVersion, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressorV3Aot(
		LanguageVersion languageVersion,
		string[] sources,
		DiagnosticAnalyzer[] suppressedAnalyzers,
		params DiagnosticResult[] diagnostics)
	{
		if (languageVersion < LanguageVersion.CSharp13)
			languageVersion = LanguageVersion.CSharp13;

		var testAot = new TestV3Aot(languageVersion);

		foreach (var suppressedAnalyzer in suppressedAnalyzers)
			testAot.AddDiagnosticAnalyzer(suppressedAnalyzer);

		foreach (var source in sources)
			testAot.TestState.Sources.Add(source);

		testAot.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		testAot.TestState.OutputKind = OutputKind.ConsoleApplication;
		testAot.TestState.Sources.Add("internal class Program { public static void Main() { } }");
		await testAot.RunAsync();
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV3NonAot(
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV3NonAot(LanguageVersion.CSharp6, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress another analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="suppressedAnalyzer">The analyzer that is expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static Task VerifySuppressorV3NonAot(
		LanguageVersion languageVersion,
		string source,
		DiagnosticAnalyzer suppressedAnalyzer,
		params DiagnosticResult[] diagnostics) =>
			VerifySuppressorV3NonAot(languageVersion, [source], [suppressedAnalyzer], diagnostics);

	/// <summary>
	/// Verify that an analyzer was used to suppress one or more other analyzers. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="suppressedAnalyzers">The analyzer(s) that are expected to be suppressed</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifySuppressorV3NonAot(
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
		await test.RunAsync();
	}

	/// <summary>
	/// Verify that an analyzer was used to suppress a compiler warning. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCompilerWarningSuppressorV3(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCompilerWarningSuppressorV3NonAot(languageVersion, sources, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyCompilerWarningSuppressorV3Aot(languageVersion, sources, diagnostics);
#endif
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress a compiler warning. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCompilerWarningSuppressorV3Aot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		if (languageVersion < LanguageVersion.CSharp13)
			languageVersion = LanguageVersion.CSharp13;

		var testAot = new TestV3Aot(languageVersion);

		foreach (var source in sources)
			testAot.TestState.Sources.Add(source);

		testAot.CompilerDiagnostics = CompilerDiagnostics.Warnings;
		testAot.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		testAot.TestState.OutputKind = OutputKind.ConsoleApplication;
		testAot.TestState.Sources.Add("internal class Program { public static void Main() { } }");
		await testAot.RunAsync();
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that an analyzer was used to suppress a compiler warning. Runs against
	/// xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the suppression</param>
	public static async Task VerifyCompilerWarningSuppressorV3NonAot(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.CompilerDiagnostics = CompilerDiagnostics.Warnings;
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		test.TestState.OutputKind = OutputKind.ConsoleApplication;
		test.TestState.Sources.Add("internal class Program { public static void Main() { } }");
		await test.RunAsync();
	}
}
