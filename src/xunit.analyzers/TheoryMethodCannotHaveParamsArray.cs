using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodCannotHaveParamsArray : DiagnosticAnalyzer
    {
        readonly XunitCapabilitiesFactory capabilitiesFactory;

        public TheoryMethodCannotHaveParamsArray() : this(XunitCapabilities.Create) { }

        // For testing
        public TheoryMethodCannotHaveParamsArray(XunitCapabilities capabilities) : this(c => capabilities) { }

        private TheoryMethodCannotHaveParamsArray(XunitCapabilitiesFactory capabilitiesFactory)
        {
            this.capabilitiesFactory = capabilitiesFactory;
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X1022_TheoryMethodCannotHaveParameterArray);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var compilation = compilationStartContext.Compilation;
                var capabilities = capabilitiesFactory(compilation);
                if (capabilities.TheorySupportsParameterArrays)
                    return;

                var theoryType = compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
                if (theoryType == null)
                    return;

                compilationStartContext.RegisterSymbolAction(symbolContext =>
                {
                    var method = (IMethodSymbol)symbolContext.Symbol;
                    var parameter = method.Parameters.LastOrDefault();
                    if (!(parameter?.IsParams ?? false))
                        return;

                    var attributes = method.GetAttributes();
                    if (attributes.ContainsAttributeType(theoryType))
                    {
                        symbolContext.ReportDiagnostic(Diagnostic.Create(
                            Constants.Descriptors.X1022_TheoryMethodCannotHaveParameterArray,
                            parameter.DeclaringSyntaxReferences.First().GetSyntax(compilationStartContext.CancellationToken).GetLocation(),
                            method.Name,
                            method.ContainingType.ToDisplayString(),
                            parameter.Name));
                    }
                }, SymbolKind.Method);
            });
        }
    }
}
