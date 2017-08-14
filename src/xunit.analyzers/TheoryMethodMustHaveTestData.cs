using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodMustHaveTestData : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1003_TheoryMethodMustHaveTestData);

        public override void Initialize(AnalysisContext context)
        {
            var typesContext = context.RequireTypes(
                Constants.Types.XunitTheoryAttribute,
                Constants.Types.XunitSdkDataAttribute);

            typesContext.RegisterSymbolAction(symbolContext =>
            {
                var theoryType = symbolContext.Compilation.GetTheoryAttributeType();
                var dataType = symbolContext.Compilation.GetDataAttributeType();

                var symbol = (IMethodSymbol)symbolContext.Symbol;
                var attributes = symbol.GetAttributes();
                if (attributes.ContainsAttributeType(theoryType) &&
                    (attributes.Length == 1 || !attributes.ContainsAttributeType(dataType)))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1003_TheoryMethodMustHaveTestData, symbol.Locations.First()));
                }
            }, SymbolKind.Method);
        }
    }
}
