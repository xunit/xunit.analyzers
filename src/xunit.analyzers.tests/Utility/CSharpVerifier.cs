using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

public class CSharpVerifier<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	/// <summary>
	/// Creates a diagnostic result for the diagnostic referenced in <see cref="TAnalyzer"/>.
	/// </summary>
	public static DiagnosticResult Diagnostic() =>
		CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XunitVerifier>.Diagnostic();

	/// <summary>
	/// Creates a diagnostic result for the given diagnostic ID.
	/// </summary>
	/// <param name="diagnosticId">The diagnostic ID</param>
	public static DiagnosticResult Diagnostic(string diagnosticId) =>
		CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XunitVerifier>.Diagnostic(diagnosticId);

	/// <summary>
	/// Creates a diagnostic result for an expected compiler error.
	/// </summary>
	/// <param name="errorIdentifier">The compiler error code (e.g., CS0619)</param>
	public static DiagnosticResult CompilerError(string errorIdentifier) =>
		new(errorIdentifier, DiagnosticSeverity.Error);

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
		await VerifyAnalyzerV2(source, diagnostics);
		await VerifyAnalyzerV3(source, diagnostics);
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
		await VerifyAnalyzerV2(languageVersion, source, diagnostics);
		await VerifyAnalyzerV3(languageVersion, source, diagnostics);
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
		await VerifyAnalyzerV2(sources, diagnostics);
		await VerifyAnalyzerV3(sources, diagnostics);
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

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzer(
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(sources, diagnostics);
		await VerifyAnalyzerV3(sources, diagnostics);
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
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2(languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3(languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtility(
		string source,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(source, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(source, diagnostics);
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
		await VerifyAnalyzerV2RunnerUtility(languageVersion, source, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(languageVersion, source, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtility(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(sources, diagnostics);
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
		await VerifyAnalyzerV2RunnerUtility(languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static async Task VerifyAnalyzerRunnerUtility(
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(sources, diagnostics);
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
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyAnalyzerV2RunnerUtility(languageVersion, sources, diagnostics);
		await VerifyAnalyzerV3RunnerUtility(languageVersion, sources, diagnostics);
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2(LanguageVersion.CSharp6, new[] { source }, diagnostics);

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
			VerifyAnalyzerV2(languageVersion, new[] { source }, diagnostics);

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

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2(
		(string filename, string content)[] sources,
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
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2(languageVersion);
		test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(LanguageVersion.CSharp6, new[] { source }, diagnostics);

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
			VerifyAnalyzerV2RunnerUtility(languageVersion, new[] { source }, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(LanguageVersion.CSharp6, sources, diagnostics);

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
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2RunnerUtility(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV2RunnerUtility(LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV2RunnerUtility(
		LanguageVersion languageVersion,
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2RunnerUtility(languageVersion);
		test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3(LanguageVersion.CSharp6, new[] { source }, diagnostics);

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
			VerifyAnalyzerV3(languageVersion, new[] { source }, diagnostics);

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

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3(
		(string filename, string content)[] sources,
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
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3(languageVersion);
		test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtility(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtility(LanguageVersion.CSharp6, new[] { source }, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="source">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtility(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtility(languageVersion, new[] { source }, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtility(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtility(LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtility(
		LanguageVersion languageVersion,
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3RunnerUtility(languageVersion);

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtility(
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerV3RunnerUtility(LanguageVersion.CSharp6, sources, diagnostics);

	/// <summary>
	/// Runs code for analysis, against xUnit.net v3 Runner Utility, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="sources">The code to verify</param>
	/// <param name="diagnostics">The expected diagnostics (pass none for code that
	/// should not trigger)</param>
	public static Task VerifyAnalyzerV3RunnerUtility(
		LanguageVersion languageVersion,
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3RunnerUtility(languageVersion);
		test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFix(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2(before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3(before, after, fixerActionKey, diagnostics);
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 and v3, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFix(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2(languageVersion, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3(languageVersion, before, after, fixerActionKey, diagnostics);
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 and v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixRunnerUtility(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2RunnerUtility(before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3RunnerUtility(before, after, fixerActionKey, diagnostics);
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 and v3 Runner Utility, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixRunnerUtility(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2RunnerUtility(languageVersion, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3RunnerUtility(languageVersion, before, after, fixerActionKey, diagnostics);
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV2(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV2(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV2(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		var newLine = FormattingOptions.NewLine.DefaultValue;

		var test = new TestV2(languageVersion)
		{
			TestCode = before.Replace("\n", newLine),
			FixedCode = after.Replace("\n", newLine),
			CodeActionEquivalenceKey = fixerActionKey,
		};
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV2RunnerUtility(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV2RunnerUtility(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 Runner Utility, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV2RunnerUtility(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		var newLine = FormattingOptions.NewLine.DefaultValue;

		var test = new TestV2RunnerUtility(languageVersion)
		{
			TestCode = before.Replace("\n", newLine),
			FixedCode = after.Replace("\n", newLine),
			CodeActionEquivalenceKey = fixerActionKey,
		};
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV3(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV3(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		var newLine = FormattingOptions.NewLine.DefaultValue;

		var test = new TestV3(languageVersion)
		{
			TestCode = before.Replace("\n", newLine),
			FixedCode = after.Replace("\n", newLine),
			CodeActionEquivalenceKey = fixerActionKey,
		};
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV3RunnerUtility(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3RunnerUtility(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3 Runner Utility, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV3RunnerUtility(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		var newLine = FormattingOptions.NewLine.DefaultValue;

		var test = new TestV3RunnerUtility(languageVersion)
		{
			TestCode = before.Replace("\n", newLine),
			FixedCode = after.Replace("\n", newLine),
			CodeActionEquivalenceKey = fixerActionKey,
		};
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	class TestBase<TVerifier> : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, TVerifier>
		where TVerifier : XunitVerifier, new()
	{
		protected TestBase(
			LanguageVersion languageVersion,
			ReferenceAssemblies referenceAssemblies)
		{
			LanguageVersion = languageVersion;
			ReferenceAssemblies = referenceAssemblies;

			// Diagnostics are reported in both normal and generated code
			TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;

			// Tests that check for messages should run independent of current system culture.
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
		}

		public LanguageVersion LanguageVersion { get; }

		protected override IEnumerable<CodeFixProvider> GetCodeFixProviders()
		{
			var analyzer = new TAnalyzer();

			foreach (var provider in CodeFixProviderDiscovery.GetCodeFixProviders(Language))
				if (analyzer.SupportedDiagnostics.Any(diagnostic => provider.FixableDiagnosticIds.Contains(diagnostic.Id)))
					yield return provider;
		}

		protected override ParseOptions CreateParseOptions() =>
			new CSharpParseOptions(LanguageVersion, DocumentationMode.Diagnose);
	}

	class TestV2 : TestBase<XunitVerifierV2>
	{
		public TestV2(LanguageVersion languageVersion) :
			base(languageVersion, CodeAnalyzerHelper.CurrentXunitV2)
		{ }
	}

	class TestV2RunnerUtility : TestBase<XunitVerifierV2>
	{
		public TestV2RunnerUtility(LanguageVersion languageVersion) :
			base(languageVersion, CodeAnalyzerHelper.CurrentXunitV2RunnerUtility)
		{ }
	}

	class TestV3 : TestBase<XunitVerifierV3>
	{
		public TestV3(LanguageVersion languageVersion) :
			base(languageVersion, CodeAnalyzerHelper.CurrentXunitV3)
		{ }
	}

	class TestV3RunnerUtility : TestBase<XunitVerifierV3>
	{
		public TestV3RunnerUtility(LanguageVersion languageVersion) :
			base(languageVersion, CodeAnalyzerHelper.CurrentXunitV3RunnerUtility)
		{ }
	}
}
