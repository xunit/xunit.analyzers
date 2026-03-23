using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Testing;

partial class CSharpVerifier<TAnalyzer>
{
	// ----- Multi-version -----

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixFixAll(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2FixAll(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3FixAll(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
	}

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixFixAll(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2FixAll(languageVersion, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3FixAll(languageVersion, before, after, fixerActionKey, diagnostics);
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixFixAllAot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2FixAll(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3FixAllAot(LanguageVersion.CSharp13, before, after, fixerActionKey, diagnostics);
	}

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixFixAllAot(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2FixAll(languageVersion, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3FixAllAot(languageVersion, before, after, fixerActionKey, diagnostics);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2 and v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixFixAllNonAot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2FixAll(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3FixAllNonAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
	}

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2 and v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixFixAllNonAot(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV2FixAll(languageVersion, before, after, fixerActionKey, diagnostics);
		await VerifyCodeFixV3FixAllNonAot(languageVersion, before, after, fixerActionKey, diagnostics);
	}

	// ----- v2 -----

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV2FixAll(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV2FixAll(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v2, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV2FixAll(
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
			CodeActionEquivalenceKey = fixerActionKey,
		};
		test.FixedState.Sources.Add(after.Replace("\n", newLine));
		test.FixedState.ExpectedDiagnostics.AddRange(diagnostics);
		test.BatchFixedState.Sources.Add(after.Replace("\n", newLine));
		test.BatchFixedState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	// ----- v3 -----

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixV3FixAll(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV3FixAllNonAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyCodeFixV3FixAllAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);
#endif
	}

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// If <paramref name="languageVersion"/> is less than 13, then AOT tests will be run
	/// against version 13 (the minimum required for .NET 9).
	/// </remarks>
	public static async Task VerifyCodeFixV3FixAll(
		LanguageVersion languageVersion,
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics)
	{
		await VerifyCodeFixV3FixAllNonAot(languageVersion, before, after, fixerActionKey, diagnostics);
#if NETCOREAPP && ROSLYN_LATEST
		await VerifyCodeFixV3FixAllAot(languageVersion, before, after, fixerActionKey, diagnostics);
#endif
	}

#if NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	/// <remarks>
	/// AOT tests will be run against C# 13 (the minimum required for .NET 9).
	/// </remarks>
	public static Task VerifyCodeFixV3FixAllAot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3FixAllAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixV3FixAllAot(
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
			CodeActionEquivalenceKey = fixerActionKey,
		};
		testAot.FixedState.Sources.Add(after.Replace("\n", newLine));
		testAot.FixedState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.BatchFixedState.Sources.Add(after.Replace("\n", newLine));
		testAot.BatchFixedState.ExpectedDiagnostics.AddRange(diagnostics);
		testAot.DisabledDiagnostics.Add("CS1701");  // assert is net9, core is net8, ignore version drift
		await testAot.RunAsync();
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v3, using C# 6.
	/// </summary>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static Task VerifyCodeFixV3FixAllNonAot(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixV3FixAllNonAot(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	/// <summary>
	/// Verify that "Fix All" correctly applies fixes to all diagnostics in a document.
	/// Runs against xUnit.net v3, using the provided version of C#.
	/// </summary>
	/// <param name="languageVersion">The language version to compile with</param>
	/// <param name="before">The code before the fix (should contain multiple diagnostics)</param>
	/// <param name="after">The expected code after all fixes are applied</param>
	/// <param name="fixerActionKey">The key of the fix to run</param>
	/// <param name="diagnostics">Any expected diagnostics that still exist after the fix</param>
	public static async Task VerifyCodeFixV3FixAllNonAot(
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
			CodeActionEquivalenceKey = fixerActionKey,
		};
		test.FixedState.Sources.Add(after.Replace("\n", newLine));
		test.FixedState.ExpectedDiagnostics.AddRange(diagnostics);
		test.BatchFixedState.Sources.Add(after.Replace("\n", newLine));
		test.BatchFixedState.ExpectedDiagnostics.AddRange(diagnostics);
		await test.RunAsync();
	}
}
