using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Testing;

public partial class CSharpVerifier<TAnalyzer>
{
	// ----- Multi-version -----

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
		await VerifyCodeFixV2RunnerUtility(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3RunnerUtility(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
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

	// ----- v2 -----

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

	// ----- v3 -----

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static Task VerifyCodeFixV3RunnerUtility(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3RunnerUtility(includeAot: true, LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3 Runner Utility, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static Task VerifyCodeFixV3RunnerUtility(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3RunnerUtility(includeAot: true, languageVersion, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3 Runner Utility, using C# 6.
	/// </summary>
	/// <param name="includeAot">Set to <see langword="false"/> to exclude testing against AOT</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// If <paramref name="includeAot"/> is true, then AOT tests will be run against C# version 13
	/// (the minimum required for .NET 9).
	/// </remarks>
	public static Task VerifyCodeFixV3RunnerUtility(
		bool includeAot,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3RunnerUtility(includeAot, LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3 Runner Utility, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="includeAot">Set to <see langword="false"/> to exclude testing against AOT</param>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13 and <paramref name="includeAot"/> is true,
	/// then AOT tests will be run against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixV3RunnerUtility(
		bool includeAot,
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
		await test.RunAsync();

#if NETCOREAPP && ROSLYN_LATEST
		if (!includeAot)
			return;

		if (languageVersion < LanguageVersion.CSharp13)
			languageVersion = LanguageVersion.CSharp13;

		var testAot = new TestV3RunnerUtilityAot(languageVersion)
		{
			TestCode = before.Replace("\n", newLine),
			FixedCode = after.Replace("\n", newLine),
			CodeActionEquivalenceKey = fixerActionKey,
		};
		testAot.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		await testAot.RunAsync();
#endif
	}
}
