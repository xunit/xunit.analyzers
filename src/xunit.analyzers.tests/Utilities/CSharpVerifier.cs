using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

public class CSharpVerifier<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	public static DiagnosticResult Diagnostic() =>
		CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic();

	public static DiagnosticResult Diagnostic(string diagnosticId) =>
		CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic(diagnosticId);

	public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) =>
		new(descriptor);

	public static DiagnosticResult CompilerError(string errorIdentifier) =>
		new(errorIdentifier, DiagnosticSeverity.Error);

	public static Task VerifyAnalyzerAsync(
		string source,
		params DiagnosticResult[] diagnostics) =>
			VerifyAnalyzerAsync(new[] { source }, diagnostics);

	public static Task VerifyAnalyzerAsync(
		string[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new Test();

		foreach (var source in sources)
			test.TestState.Sources.Add(source);

		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	public static Task VerifyAnalyzerAsync(
		(string filename, string content)[] sources,
		params DiagnosticResult[] diagnostics)
	{
		var test = new Test();
		test.TestState.Sources.AddRange(sources.Select(s => (s.filename, SourceText.From(s.content))));
		test.TestState.ExpectedDiagnostics.AddRange(diagnostics);
		return test.RunAsync();
	}

	public static Task VerifyCodeFixAsync(
		string before,
		string after,
		int? codeActionIndex = null) =>
			new Test
			{
				TestCode = before,
				FixedCode = after,
				CodeActionIndex = codeActionIndex
			}.RunAsync();

	public class Test : CSharpCodeFixTest<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>
	{
		public Test()
		{
			ReferenceAssemblies = CodeAnalyzerHelper.CurrentXunit;

			// xunit diagnostics are reported in both normal and generated code
			TestBehaviors |= TestBehaviors.SkipGeneratedCodeCheck;
		}

		protected override IEnumerable<CodeFixProvider> GetCodeFixProviders()
		{
			var analyzer = new TAnalyzer();

			foreach (var provider in CodeFixProviderDiscovery.GetCodeFixProviders(Language))
				if (analyzer.SupportedDiagnostics.Any(diagnostic => provider.FixableDiagnosticIds.Contains(diagnostic.Id)))
					yield return provider;
		}
	}
}
