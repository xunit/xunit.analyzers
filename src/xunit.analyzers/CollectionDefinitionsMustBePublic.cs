using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CollectionDefinitionClassesMustBePublic : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1027_CollectionDefinitionClassMustBePublic);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (xunitContext.V2Core?.CollectionDefinitionAttributeType is null)
					return;
				if (context.Symbol.DeclaredAccessibility == Accessibility.Public)
					return;
				if (context.Symbol is not INamedTypeSymbol classSymbol)
					return;

				var doesClassContainCollectionDefinitionAttribute =
					classSymbol
						.GetAttributes()
						.Any(a => xunitContext.V2Core.CollectionDefinitionAttributeType.IsAssignableFrom(a.AttributeClass));

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
}
