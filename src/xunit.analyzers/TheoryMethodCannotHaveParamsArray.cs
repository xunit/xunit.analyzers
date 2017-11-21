using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodCannotHaveParamsArray : XunitDiagnosticAnalyzer
    {
        public TheoryMethodCannotHaveParamsArray() { }

        /// <summary>For testing purposes only.</summary>
        public TheoryMethodCannotHaveParamsArray(string assemblyVersion) : base(new Version(assemblyVersion)) { }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1022_TheoryMethodCannotHaveParameterArray);

        protected override bool ShouldAnalyze(XunitContext xunitContext)
        {
            return xunitContext.HasCoreReference && !xunitContext.Core.TheorySupportsParameterArrays;
        }

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var method = (IMethodSymbol)symbolContext.Symbol;
                var parameter = method.Parameters.LastOrDefault();
                if (!(parameter?.IsParams ?? false))
                    return;

                var attributes = method.GetAttributes();
                if (attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
                {
                    symbolContext.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X1022_TheoryMethodCannotHaveParameterArray,
                        parameter.DeclaringSyntaxReferences.First().GetSyntax(compilationStartContext.CancellationToken).GetLocation(),
                        method.Name,
                        method.ContainingType.ToDisplayString(),
                        parameter.Name));
                }
            }, SymbolKind.Method);
        }
    }
}
