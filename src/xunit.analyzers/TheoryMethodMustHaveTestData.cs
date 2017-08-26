using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodMustHaveTestData : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1003_TheoryMethodMustHaveTestData);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var symbol = (IMethodSymbol)symbolContext.Symbol;
                var attributes = symbol.GetAttributes();
                if (attributes.ContainsAttributeType(xunitContext.TheoryAttributeType) &&
                    (attributes.Length == 1 || !attributes.ContainsAttributeType(xunitContext.DataAttributeType)))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1003_TheoryMethodMustHaveTestData, symbol.Locations.First()));
                }
            }, SymbolKind.Method);
        }
    }
}
