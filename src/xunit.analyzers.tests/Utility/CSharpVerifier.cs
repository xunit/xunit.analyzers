using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

public partial class CSharpVerifier<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	/// <summary>
	/// Creates a diagnostic result for the diagnostic referenced in <typeparamref name="TAnalyzer"/>.
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

	class TestBase<TVerifier> : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, TVerifier>
		where TVerifier : XunitVerifier, new()
	{
		List<DiagnosticAnalyzer> additionalDiagnosticAnalyzers = new();

		protected TestBase(
			LanguageVersion languageVersion,
			ReferenceAssemblies referenceAssemblies)
		{
			LanguageVersion = languageVersion;
			ReferenceAssemblies = referenceAssemblies;

			// Ensure all fixed source matches the inline source (tabs, not spaces)
			TestState.AnalyzerConfigFiles.Add(
				(
					"/.editorconfig",
					SourceText.From("""
						[*]
						indent_style = tab
						""")
				)
			);

			// Diagnostics are reported in both normal and generated code
			TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;

			// Tests that check for messages should run independent of current system culture.
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
		}

		public LanguageVersion LanguageVersion { get; }

		public void AddDiagnosticAnalyzer(DiagnosticAnalyzer analyzer) =>
			additionalDiagnosticAnalyzers.Add(analyzer);

		protected override IEnumerable<CodeFixProvider> GetCodeFixProviders()
		{
			var analyzer = new TAnalyzer();

			foreach (var provider in CodeFixProviderDiscovery.GetCodeFixProviders(Language))
				if (analyzer.SupportedDiagnostics.Any(diagnostic => provider.FixableDiagnosticIds.Contains(diagnostic.Id)))
					yield return provider;
		}

		protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers()
		{
			yield return new TAnalyzer();

			foreach (var diagnosticAnalyzer in additionalDiagnosticAnalyzers)
				yield return diagnosticAnalyzer;
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
