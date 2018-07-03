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

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, 
            XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(context =>
            {
                if (context.Symbol.DeclaredAccessibility == Accessibility.Public)
                    return;

                var classSymbol = (INamedTypeSymbol) context.Symbol;

                var doesClassContainCollectionDefinitionAttribute = classSymbol
                    .GetAttributes()
                    .Any(a => xunitContext.Core.CollectionDefinitionAttributeType.IsAssignableFrom(a.AttributeClass));

                if (!doesClassContainCollectionDefinitionAttribute)
                    return;

                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.X1027_CollectionDefinitionClassMustBePublic,
                    classSymbol.Locations.First(),
                    classSymbol.Locations.Skip(1),
                    classSymbol.Name));

            }, SymbolKind.NamedType);
        }
    }
}
