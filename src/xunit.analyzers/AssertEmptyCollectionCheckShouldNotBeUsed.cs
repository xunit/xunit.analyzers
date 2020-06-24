using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEmptyCollectionCheckShouldNotBeUsed : AssertUsageAnalyzerBase
	{
		public AssertEmptyCollectionCheckShouldNotBeUsed()
			: base(Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed, new[] { "Collection" })
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var invocation = (InvocationExpressionSyntax)invocationOperation.Syntax;
			var arguments = invocation.ArgumentList.Arguments;
			if (arguments.Count != 1)
				return;

			if (!method.Parameters[0].Type.OriginalDefinition.SpecialType.Equals(SpecialType.System_Collections_Generic_IEnumerable_T))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed,
					invocationOperation.Syntax.GetLocation(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
