using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class FactMethodShouldNotHaveTestData : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1005_FactMethodShouldNotHaveTestData);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var symbol = (IMethodSymbol)symbolContext.Symbol;
                var attributes = symbol.GetAttributes();
                if (attributes.Length > 1 &&
                    attributes.ContainsAttributeType(xunitContext.Core.FactAttributeType, exactMatch: true) &&
                    !attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType) &&
                    attributes.ContainsAttributeType(xunitContext.Core.DataAttributeType))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1005_FactMethodShouldNotHaveTestData, symbol.Locations.First()));
                }
            }, SymbolKind.Method);
        }
    }
}
