using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FactMethodShouldNotHaveTestData : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1005_FactMethodShouldNotHaveTestData);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var factType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
                var theoryType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
                var dataType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute);
                if (factType == null || theoryType == null || dataType == null)
                    return;

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var symbol = (IMethodSymbol)symbolContext.Symbol;
                    var attributes = symbol.GetAttributes();
                    if (attributes.Length > 1 && 
                        attributes.ContainsAttributeType(factType, exactMatch: true) &&
                        !attributes.ContainsAttributeType(theoryType) &&
                        attributes.ContainsAttributeType(dataType))
                    {
                        symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1005_FactMethodShouldNotHaveTestData, symbol.Locations.First()));
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
