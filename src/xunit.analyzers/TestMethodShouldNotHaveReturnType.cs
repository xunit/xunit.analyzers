using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TestMethodShouldNotHaveReturnType : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.X1027_TestMethodShouldNotHaveReturnType);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            var compilation = compilationStartContext.Compilation;
            var taskType = compilation.GetTypeByMetadataName(Constants.Types.SystemThreadingTasksTask);

            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
                // NOTE: This analyzer should fire even if the return type is a derived type of Task, such as Task<int>.
                if (methodSymbol.ReturnsVoid || (taskType != null && methodSymbol.ReturnType == taskType))
                    return;

                var attributes = methodSymbol.GetAttributes();
                if (!attributes.ContainsAttributeType(xunitContext.FactAttributeType, exactMatch: true) &&
                    !attributes.ContainsAttributeType(xunitContext.TheoryAttributeType, exactMatch: true))
                    return;

                symbolContext.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.X1027_TestMethodShouldNotHaveReturnType,
                    methodSymbol.Locations.First(),
                    methodSymbol.Name,
                    methodSymbol.ContainingType.Name));
            }, SymbolKind.Method);
        }
    }
}
