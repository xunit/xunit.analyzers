using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GenericCollectionDefinitionsInAot() :
	XunitV3AotDiagnosticAnalyzer(Descriptors.X1058_GenericCollectionDefinitionNotSupported)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var collectionDefinitionAttributeType = xunitContext.V3Core?.CollectionDefinitionAttributeType;
		if (collectionDefinitionAttributeType is null)
			return;

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol collectionType
					|| !collectionType.IsGenericType
					|| !collectionType.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, collectionDefinitionAttributeType)))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1058_GenericCollectionDefinitionNotSupported,
					collectionType.Locations.FirstOrDefault()
				)
			);
		}, SymbolKind.NamedType);
	}
}
