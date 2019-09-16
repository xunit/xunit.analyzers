using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodShouldHaveParameters : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1006_TheoryMethodShouldHaveParameters);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var symbol = (IMethodSymbol)symbolContext.Symbol;
                if (symbol.Parameters.Length > 0)
                    return;

                var attributes = symbol.GetAttributes();
                if (attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1006_TheoryMethodShouldHaveParameters, symbol.Locations.First()));
                }
            }, SymbolKind.Method);
        }
    }
}
