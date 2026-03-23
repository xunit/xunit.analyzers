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
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
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
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
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
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzer(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3(languageVersion, sources, diagnostics);
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
		await VerifyAnalyzerV2(languageVersion, [source], diagnostics);
		await VerifyAnalyzerV3NonAot(languageVersion, [source], diagnostics);
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
		await VerifyAnalyzerV2(LanguageVersion.CSharp6, sources, diagnostics);
		await VerifyAnalyzerV3NonAot(LanguageVersion.CSharp6, sources, diagnostics);
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
		await VerifyAnalyzerV2(languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3NonAot(languageVersion, sources, diagnostics);
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
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyAnalyzerV3(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV3NonAot(LanguageVersion.CSharp6, source, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyAnalyzerV3Aot(LanguageVersion.CSharp13, source, diagnostics);
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
		await VerifyAnalyzerV3NonAot(languageVersion, source, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyAnalyzerV3Aot(languageVersion, source, diagnostics);
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
		await VerifyAnalyzerV3NonAot(LanguageVersion.CSharp6, sources, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyAnalyzerV3Aot(LanguageVersion.CSharp13, sources, diagnostics);
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
		await VerifyAnalyzerV3NonAot(languageVersion, sources, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyAnalyzerV3Aot(languageVersion, sources, diagnostics);
#endif
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using C# 13.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(LanguageVersion.CSharp13, [source], diagnostics);

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
			VerifyAnalyzerV3Aot(languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 (Native AOT), using C# 13.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3Aot(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3Aot(LanguageVersion.CSharp13, sources, diagnostics);

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
		params DiagnosticResult[] diagnostics)
	{
		// We might be called from an API with a lower default C# version, so bump up to the minimum
		if (languageVersion < LanguageVersion.CSharp13)
			languageVersion = LanguageVersion.CSharp13;

		var testAot = new TestV3Aot(languageVersion);

		foreach (var source in sources)
			testAot.TestState.Sources.Add(source);

		testAot.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		return testAot.RunAsync();
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(LanguageVersion.CSharp6, [source], diagnostics);

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
			VerifyAnalyzerV3NonAot(languageVersion, [source], diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3NonAot(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3NonAot(LanguageVersion.CSharp6, sources, diagnostics);

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
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}
}
