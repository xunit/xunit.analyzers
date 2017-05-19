using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodCannotHaveDefaultParameter : DiagnosticAnalyzer
    {
        readonly XunitCapabilitiesFactory capabilitiesFactory;

        public TheoryMethodCannotHaveDefaultParameter() : this(XunitCapabilities.Create) { }

        // For testing
        public TheoryMethodCannotHaveDefaultParameter(XunitCapabilities capabilities) : this(c => capabilities) { }

        private TheoryMethodCannotHaveDefaultParameter(XunitCapabilitiesFactory capabilitiesFactory)
        {
            this.capabilitiesFactory = capabilitiesFactory;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var capabilities = capabilitiesFactory(compilation);
                if (capabilities.TheorySupportsDefaultParameterValues)
                    return;

                var theoryType = compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
                if (theoryType == null)
                    return;

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var method = (IMethodSymbol)symbolContext.Symbol;
                    var attributes = method.GetAttributes();
                    if (!attributes.ContainsAttributeType(theoryType))
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
                                Constants.Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter,
                                syntaxNode.Default.GetLocation(),
                                method.Name,
                                method.ContainingType.ToDisplayString(),
                                parameter.Name));
                        }
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
