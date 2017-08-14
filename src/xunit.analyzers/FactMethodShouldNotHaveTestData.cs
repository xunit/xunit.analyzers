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
            var typesContext = context.RequireTypes(
                Constants.Types.XunitFactAttribute,
                Constants.Types.XunitTheoryAttribute,
                Constants.Types.XunitSdkDataAttribute);

            typesContext.RegisterSymbolAction(symbolContext =>
            {
                var factType = symbolContext.Compilation.GetFactAttributeType();
                var theoryType = symbolContext.Compilation.GetTheoryAttributeType();
                var dataType = symbolContext.Compilation.GetDataAttributeType();

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
        }
    }
}
