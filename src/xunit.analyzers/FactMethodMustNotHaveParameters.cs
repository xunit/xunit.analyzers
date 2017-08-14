using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FactMethodMustNotHaveParameters : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Descriptors.X1001_FactMethodMustNotHaveParameters);

        public override void Initialize(AnalysisContext context)
        {
            context.RequireTypes(Constants.Types.XunitFactAttribute).RegisterSyntaxNodeAction(syntaxContext =>
            {
                var methodDeclaration = syntaxContext.Node as MethodDeclarationSyntax;
                if (methodDeclaration.ParameterList.Parameters.Count == 0)
                    return;

                var factType = syntaxContext.Compilation().GetFactAttributeType();
                if (methodDeclaration.AttributeLists.ContainsAttributeType(syntaxContext.SemanticModel, factType, exactMatch: true))
                {
                    syntaxContext.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X1001_FactMethodMustNotHaveParameters,
                        methodDeclaration.Identifier.GetLocation(),
                        methodDeclaration.Identifier.ValueText));
                }
            }, SyntaxKind.MethodDeclaration);
        }
    }
}
