using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers;

namespace Xunit.Suppressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsiderCallingConfigureAwaitSuppressor : XunitDiagnosticSuppressor
{
	public ConsiderCallingConfigureAwaitSuppressor() :
		base(Descriptors.CA2007_Suppression)
	{ }

	protected override bool ShouldSuppress(
		Diagnostic diagnostic,
		SuppressionAnalysisContext context,
		XunitContext xunitContext)
	{
		if (diagnostic.Location.SourceTree is null)
			return false;

		var factAttributeType = xunitContext.Core.FactAttributeType;
		var theoryAttributeType = xunitContext.Core.TheoryAttributeType;
		if (factAttributeType is null || theoryAttributeType is null)
			return false;

		var root = diagnostic.Location.SourceTree.GetRoot(context.CancellationToken);
		if (root?.FindNode(diagnostic.Location.SourceSpan) is not InvocationExpressionSyntax invocationSyntax)
			return false;

		var current = invocationSyntax.Parent;
		while (true)
		{
			if (current is null || current is LocalFunctionStatementSyntax || current is LambdaExpressionSyntax)
				return false;
			if (current is MethodDeclarationSyntax)
				break;

			current = current.Parent;
		}

		var semanticModel = context.GetSemanticModel(diagnostic.Location.SourceTree);
		var methodSymbol = semanticModel.GetDeclaredSymbol(current);
		if (methodSymbol is null)
			return false;

		var attributes = ImmutableHashSet.Create(SymbolEqualityComparer.Default, factAttributeType, theoryAttributeType);

		return
			methodSymbol
				.GetAttributes()
				.Any(a => attributes.Contains(a.AttributeClass));
	}
}
