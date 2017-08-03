using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TheoryMethodMustUseAllParameters : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1026_TheoryMethodMustUseAllParameters);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var theoryType = compilationStartContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
                if (theoryType == null)
                    return;

                compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
                {
                    var methodSyntax = (MethodDeclarationSyntax)syntaxNodeContext.Node;
                    var methodSymbol = syntaxNodeContext.SemanticModel.GetDeclaredSymbol(methodSyntax);

                    var attributes = methodSymbol.GetAttributes();
                    if (!attributes.ContainsAttributeType(theoryType))
                        return;

                    AnalyzeTheoryParameters(syntaxNodeContext, methodSyntax, methodSymbol);
                },
                SyntaxKind.MethodDeclaration);
            });
        }

        private static void AnalyzeTheoryParameters(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodSyntax, IMethodSymbol methodSymbol)
        {
            var flowAnalysis = context.SemanticModel.AnalyzeDataFlow(methodSyntax.Body);
            var usedParameters = new HashSet<ISymbol>(flowAnalysis.ReadInside);

            for (int i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var parameterSymbol = methodSymbol.Parameters[i];
                if (!usedParameters.Contains(parameterSymbol))
                {
                    var parameterSyntax = methodSyntax.ParameterList.Parameters[i];

                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X1026_TheoryMethodMustUseAllParameters,
                        parameterSyntax.Identifier.GetLocation(),
                        methodSymbol.Name,
                        methodSymbol.ContainingType.Name,
                        parameterSymbol.Name));
                }
            }
        }
    }
}
