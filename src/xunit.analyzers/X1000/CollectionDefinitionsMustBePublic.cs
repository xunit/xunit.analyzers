using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CollectionDefinitionClassesMustBePublic : XunitDiagnosticAnalyzer
{
	public CollectionDefinitionClassesMustBePublic() :
		base(Descriptors.X1027_CollectionDefinitionClassMustBePublic)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.CollectionDefinitionAttributeType is null)
				return;
			if (context.Symbol.DeclaredAccessibility == Accessibility.Public)
				return;
			if (context.Symbol is not INamedTypeSymbol classSymbol)
				return;

			var doesClassContainCollectionDefinitionAttribute =
				classSymbol
					.GetAttributes()
					.Any(a => xunitContext.Core.CollectionDefinitionAttributeType.IsAssignableFrom(a.AttributeClass));

			if (!doesClassContainCollectionDefinitionAttribute)
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1027_CollectionDefinitionClassMustBePublic,
					classSymbol.Locations.First(),
					classSymbol.Locations.Skip(1)
				)
			);
		}, SymbolKind.NamedType);
	}
}
