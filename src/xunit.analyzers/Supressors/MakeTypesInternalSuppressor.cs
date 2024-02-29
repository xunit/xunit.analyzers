using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Supressors;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MakeTypesInternalSuppressor : DiagnosticSuppressor
{
	private static readonly SuppressionDescriptor Descriptor = new("xUnitSup-CA1515", "CA1515", "xUnit's test classes must be public.");

	public override void ReportSuppressions(SuppressionAnalysisContext context)
	{
		INamedTypeSymbol? factType = context.Compilation.GetTypeByMetadataName("Xunit.FactAttribute");
		INamedTypeSymbol? theoryType = context.Compilation.GetTypeByMetadataName("Xunit.TheoryAttribute");
		if (factType is null || theoryType is null)
		{
			return;
		}

		foreach (var diagnostic in context.ReportedDiagnostics)
		{
			var root = diagnostic.Location.SourceTree?.GetRoot(context.CancellationToken);
			if (root?.FindNode(diagnostic.Location.SourceSpan) is not ClassDeclarationSyntax classDeclaration)
			{
				return;
			}

			var potentialXunitAttributes = classDeclaration
				.DescendantNodes(static node => node is not BlockSyntax)
				.OfType<AttributeSyntax>()
				.Where(a => a.Name is IdentifierNameSyntax { Identifier.Text: "Fact" or "Theory" })
				.ToList();
			if(potentialXunitAttributes.Count > 0)
			{
				foreach (var potentialXunitAttribute in potentialXunitAttributes)
				{
					SemanticModel semanticModel = context.GetSemanticModel(diagnostic.Location.SourceTree!);
					ISymbol? symbol = semanticModel.GetSymbolInfo(potentialXunitAttribute).Symbol;
					if (symbol is IMethodSymbol methodSymbol)
					{
						symbol = methodSymbol.ContainingType;
					}

					if (SymbolEqualityComparer.Default.Equals(symbol, factType) || SymbolEqualityComparer.Default.Equals(symbol, theoryType))
					{
						context.ReportSuppression(Suppression.Create(Descriptor, diagnostic));
					}
				}	
			}
		}
	}

	public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = ImmutableArray.Create(Descriptor);
}
