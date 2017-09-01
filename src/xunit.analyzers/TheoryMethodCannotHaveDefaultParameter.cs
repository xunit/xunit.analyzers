using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodCannotHaveDefaultParameter : XunitDiagnosticAnalyzer
    {
        public TheoryMethodCannotHaveDefaultParameter() : base() { }

        // For testing
        public TheoryMethodCannotHaveDefaultParameter(XunitCapabilities capabilities) : base(capabilities) { }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter);

        protected override bool ShouldAnalyze(XunitContext xunitContext)
        {
            return !xunitContext.Capabilities.TheorySupportsDefaultParameterValues;
        }

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var method = (IMethodSymbol)symbolContext.Symbol;
                var attributes = method.GetAttributes();
                if (!attributes.ContainsAttributeType(xunitContext.TheoryAttributeType))
                    return;
                foreach (var parameter in method.Parameters)
                {
                    symbolContext.CancellationToken.ThrowIfCancellationRequested();
                    if (parameter.HasExplicitDefaultValue)
                    {
                        var syntaxNode = parameter.DeclaringSyntaxReferences.First()
                            .GetSyntax(compilationStartContext.CancellationToken)
                            .FirstAncestorOrSelf<ParameterSyntax>();
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                            Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter,
                            syntaxNode.Default.GetLocation(),
                            method.Name,
                            method.ContainingType.ToDisplayString(),
                            parameter.Name));
                    }
                }
            }, SymbolKind.Method);
        }
    }
}
