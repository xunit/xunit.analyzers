using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DataAttributeShouldBeUsedOnATheory : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1008_DataAttributeShouldBeUsedOnATheory);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var factType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
                var dataType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute);
                if (factType == null || dataType == null)
                    return;

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
                    var attributes = methodSymbol.GetAttributes();
                    if (attributes.Length == 0)
                        return;

                    // Instead of checking for Theory, we check for any Fact. If it is a Fact which is not a Theory,
                    // we will let other rules (i.e. FactMethodShouldNotHaveTestData) handle that case.
                    if (!attributes.ContainsAttributeType(factType) && attributes.ContainsAttributeType(dataType))
                        symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1008_DataAttributeShouldBeUsedOnATheory, methodSymbol.Locations.First()));
                }, SymbolKind.Method);
            });
        }
    }
}
