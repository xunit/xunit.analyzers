using System.Collections.Generic;
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
	public static DiagnosticResult Diagnostic() =>
		CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XunitVerifier>.Diagnostic();

	public static DiagnosticResult Diagnostic(string diagnosticId) =>
		CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XunitVerifier>.Diagnostic(diagnosticId);

	public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
		new(descriptor);

	public static DiagnosticResult CompilerError(string errorIdentifier) =>
		new(errorIdentifier, DiagnosticSeverity.Error);

	public static Task VerifyAnalyzerAsyncV2(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV2(LanguageVersion.CSharp6, new[] { source }, diagnostics);

	public static Task VerifyAnalyzerAsyncV2(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV2(languageVersion, new[] { source }, diagnostics);

	public static Task VerifyAnalyzerAsyncV2(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV2(LanguageVersion.CSharp6, sources, diagnostics);

	public static Task VerifyAnalyzerAsyncV2(
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

	public static Task VerifyAnalyzerAsyncV2(
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV2(LanguageVersion.CSharp6, sources, diagnostics);

	public static Task VerifyAnalyzerAsyncV2(
		LanguageVersion languageVersion,
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV2(languageVersion);
		test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	public static Task VerifyAnalyzerAsyncV3(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV3(LanguageVersion.CSharp6, new[] { source }, diagnostics);

	public static Task VerifyAnalyzerAsyncV3(
		LanguageVersion languageVersion,
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV3(languageVersion, new[] { source }, diagnostics);

	public static Task VerifyAnalyzerAsyncV3(
		string[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV3(LanguageVersion.CSharp6, sources, diagnostics);

	public static Task VerifyAnalyzerAsyncV3(
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

	public static Task VerifyAnalyzerAsyncV3(
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsyncV3(LanguageVersion.CSharp6, sources, diagnostics);

	public static Task VerifyAnalyzerAsyncV3(
		LanguageVersion languageVersion,
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new TestV3(languageVersion);
		test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	public static Task VerifyCodeFixAsyncV2(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixAsyncV2(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	public static Task VerifyCodeFixAsyncV2(
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

	public static Task VerifyCodeFixAsyncV3(
		string before,
		string after,
		string fixerActionKey,
		params DiagnosticResult[] diagnostics) =>
			VerifyCodeFixAsyncV3(LanguageVersion.CSharp6, before, after, fixerActionKey, diagnostics);

	public static Task VerifyCodeFixAsyncV3(
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

	public class TestBase : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, XunitVerifier>
	{
		protected TestBase(
			LanguageVersion languageVersion,
			ReferenceAssemblies referenceAssemblies)
		{
			LanguageVersion = languageVersion;
			ReferenceAssemblies = referenceAssemblies;

			// Diagnostics are reported in both normal and generated code
			TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;
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

	public class TestV2 : TestBase
	{
		public TestV2(LanguageVersion languageVersion) :
			base(languageVersion, CodeAnalyzerHelper.CurrentXunitV2)
		{ }
	}

	public class TestV3 : TestBase
	{
		public TestV3(LanguageVersion languageVersion) :
			base(languageVersion, CodeAnalyzerHelper.CurrentXunitV3)
		{ }
	}
}
