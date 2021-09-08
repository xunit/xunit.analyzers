using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Xunit.Analyzers
{
	public class CSharpVerifier<TAnalyzer>
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		public static DiagnosticResult Diagnostic()
			=> CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic();

		public static DiagnosticResult Diagnostic(string diagnosticId)
			=> CSharpCodeFixVerifier<TAnalyzer, EmptyCodeFixProvider, XUnitVerifier>.Diagnostic(diagnosticId);

		public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
			=> new DiagnosticResult(descriptor);

		public static DiagnosticResult CompilerError(string errorIdentifier)
			=> new DiagnosticResult(errorIdentifier, DiagnosticSeverity.Error);

		public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
		{
			var test = new Test { TestCode = source };
			test.ExpectedDiagnostics.AddRange(expected);
			return test.RunAsync();
		}

		public static Task VerifyCodeFixAsync(string source, string fixedSource)
			=> VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

		public static Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
			=> VerifyCodeFixAsync(source, new[] { expected }, fixedSource);

		public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
		{
			var test = new Test
			{
				TestCode = source,
				FixedCode = fixedSource,
			};

			test.ExpectedDiagnostics.AddRange(expected);
			return test.RunAsync();
		}

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
}
