using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Testing;

public partial class CSharpVerifier<TAnalyzer>
{
	// ----- Multi-version -----

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
		await VerifyCodeFixV2(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
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

	// ----- v2 -----

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

	// ----- v3 -----

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
}
