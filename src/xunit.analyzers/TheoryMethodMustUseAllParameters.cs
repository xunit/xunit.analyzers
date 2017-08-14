﻿using System.Collections.Generic;
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
            context.RequireTypes(Constants.Types.XunitTheoryAttribute).RegisterSyntaxNodeAction(syntaxContext =>
            {
                var methodSyntax = (MethodDeclarationSyntax)syntaxContext.Node;
                var methodSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(methodSyntax);

                var theoryType = syntaxContext.Compilation().GetTheoryAttributeType();
                var attributes = methodSymbol.GetAttributes();
                if (!attributes.ContainsAttributeType(theoryType))
                    return;

                AnalyzeTheoryParameters(syntaxContext, methodSyntax, methodSymbol);
            },
            SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeTheoryParameters(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodSyntax, IMethodSymbol methodSymbol)
        {
            var flowAnalysis = context.SemanticModel.AnalyzeDataFlow(methodSyntax.Body);
            var usedParameters = new HashSet<ISymbol>(flowAnalysis.ReadInside);

            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
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
