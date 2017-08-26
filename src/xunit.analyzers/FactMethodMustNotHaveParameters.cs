using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FactMethodMustNotHaveParameters : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1001_FactMethodMustNotHaveParameters);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
            {
                var methodDeclaration = syntaxNodeContext.Node as MethodDeclarationSyntax;
                if (methodDeclaration.ParameterList.Parameters.Count == 0)
                    return;

                if (methodDeclaration.AttributeLists.ContainsAttributeType(syntaxNodeContext.SemanticModel, xunitContext.FactAttributeType, exactMatch: true))
                {
                    syntaxNodeContext.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X1001_FactMethodMustNotHaveParameters,
                        methodDeclaration.Identifier.GetLocation(),
                        methodDeclaration.Identifier.ValueText));
                }
            }, SyntaxKind.MethodDeclaration);
        }
    }
}
