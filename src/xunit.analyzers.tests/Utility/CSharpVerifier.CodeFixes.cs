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
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
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
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
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

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixAot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3Aot(LanguageVersion.CSharp13, before, after, fixerActionKey, diagnostics);
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
	public static async Task VerifyCodeFixAot(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2(languageVersion, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3Aot(languageVersion, before, after, fixerActionKey, diagnostics);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixNonAot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3NonAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
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
	public static async Task VerifyCodeFixNonAot(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2(languageVersion, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3NonAot(languageVersion, before, after, fixerActionKey, diagnostics);
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
		test.FixedState.ExpectedDiagnostics.AddRange(diagnostics);
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
	/// <remarks>
	/// AOT tests will be run against C# version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixV3(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV3NonAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyCodeFixV3Aot(LanguageVersion.CSharp13, before, after, fixerActionKey, diagnostics);
#endif
	}

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3, using the
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
	public static async Task VerifyCodeFixV3(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV3NonAot(languageVersion, before, after, fixerActionKey, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyCodeFixV3Aot(languageVersion, before, after, fixerActionKey, diagnostics);
#endif
	}
#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3, using C# 13.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV3Aot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3Aot(LanguageVersion.CSharp13, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixV3Aot(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		if (languageVersion < LanguageVersion.CSharp13)
			languageVersion = LanguageVersion.CSharp13;

		var newLine = FormattingOptions.NewLine.DefaultValue;
		var testAot = new TestV3Aot(languageVersion)
		{
			TestCode = before.Replace("\n", newLine),
			FixedCode = after.Replace("\n", newLine),
			CodeActionEquivalenceKey = fixerActionKey,
		};
		testAot.FixedState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		await testAot.RunAsync();
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV3NonAot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3NonAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that a code fix has been applied. Runs against xUnit.net v3, using the
	/// provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix</param>
	/// <param name="after">The expected code after the fix</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixV3NonAot(
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
		test.FixedState.ExpectedDiagnostics.AddRange(diagnostics);
		await test.RunAsync();
	}
}
