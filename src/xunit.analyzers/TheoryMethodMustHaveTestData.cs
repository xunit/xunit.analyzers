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
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var theoryType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
                var dataType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute);
                if (theoryType == null || dataType == null)
                    return;

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var symbol = (IMethodSymbol)symbolContext.Symbol;
                    var attributes = symbol.GetAttributes();
                    if (attributes.ContainsAttributeType(theoryType) &&
                        (attributes.Length == 1 || !attributes.ContainsAttributeType(dataType)))
                    {
                        symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.X1003_TheoryMethodMustHaveTestData, symbol.Locations.First()));
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
