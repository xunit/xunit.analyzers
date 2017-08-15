using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodShouldHaveParameters : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1006_TheoryMethodShouldHaveParameters);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var theoryType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
                if (theoryType == null)
                    return;

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var symbol = (IMethodSymbol)symbolContext.Symbol;
                    if (symbol.Parameters.Length > 0)
                        return;

                    var attributes = symbol.GetAttributes();
                    if (attributes.ContainsAttributeType(theoryType))
                    {
                        symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1006_TheoryMethodShouldHaveParameters, symbol.Locations.First()));
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
