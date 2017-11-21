using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DataAttributeShouldBeUsedOnATheory : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1008_DataAttributeShouldBeUsedOnATheory);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
                var attributes = methodSymbol.GetAttributes();
                if (attributes.Length == 0)
                    return;

                // Instead of checking for Theory, we check for any Fact. If it is a Fact which is not a Theory,
                // we will let other rules (i.e. FactMethodShouldNotHaveTestData) handle that case.
                if (!attributes.ContainsAttributeType(xunitContext.Core.FactAttributeType) && attributes.ContainsAttributeType(xunitContext.Core.DataAttributeType))
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1008_DataAttributeShouldBeUsedOnATheory, methodSymbol.Locations.First()));
            }, SymbolKind.Method);
        }
    }
}
