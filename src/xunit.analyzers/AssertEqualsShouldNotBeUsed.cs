using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEqualsShouldNotBeUsed : AssertUsageAnalyzerBase
	{
		internal static string MethodName = "MethodName";
		internal const string EqualsMethod = "Equals";
		internal const string ReferenceEqualsMethod = "ReferenceEquals";

		public AssertEqualsShouldNotBeUsed()
			: base(Descriptors.X2001_AssertEqualsShouldNotBeUsed, new[] { EqualsMethod, ReferenceEqualsMethod })
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2001_AssertEqualsShouldNotBeUsed,
					invocation.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None))));
		}
	}
}
