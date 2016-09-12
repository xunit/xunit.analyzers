using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertEqualLiteralValueShouldBeFirst : DiagnosticAnalyzer
    {
        private static HashSet<string> methodNames = new HashSet<string>(StringComparer.Ordinal) { "Equal", "StrictEqual", "NotEqual", "NotStrictEqual" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var assertType = compilationContext.Compilation.GetTypeByMetadataName(Constants.Types.XunitAssert);
                if (assertType == null)
                    return;

                compilationContext.RegisterSyntaxNodeAction(syntaxContext =>
                {
                    var invocation = (InvocationExpressionSyntax)syntaxContext.Node;
                    if (invocation.ArgumentList.Arguments.Count < 2)
                        return;

                    var symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(invocation, syntaxContext.CancellationToken);
                    if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
                        return;

                    var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
                    if (methodSymbol.MethodKind != MethodKind.Ordinary ||
                        methodSymbol.ContainingType != assertType ||
                        !methodNames.Contains(methodSymbol.Name))
                        return;

                    var firstArg = invocation.ArgumentList.Arguments[0];
                    var secondArg = invocation.ArgumentList.Arguments[1];

                    if (IsLiteralOrConstant(secondArg.Expression, syntaxContext.SemanticModel, syntaxContext.CancellationToken) &&
                        !IsLiteralOrConstant(firstArg.Expression, syntaxContext.SemanticModel, syntaxContext.CancellationToken))
                    {
                        var parentMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                        var parentType = parentMethod.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                        syntaxContext.ReportDiagnostic(Diagnostic.Create(
                            Constants.Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst,
                            invocation.GetLocation(),
                            secondArg.Expression.ToString(),
                            SymbolDisplay.ToDisplayString(methodSymbol, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None).WithParameterOptions(SymbolDisplayParameterOptions.IncludeName)),
                            parentMethod.Identifier.ValueText,
                            parentType.Identifier.ValueText));
                    }
                }, SyntaxKind.InvocationExpression);
            });
        }

        static bool IsLiteralOrConstant(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            return expression.IsKind(SyntaxKind.DefaultExpression) ||
                   expression.IsKind(SyntaxKind.TypeOfExpression) ||
                   expression.IsKind(SyntaxKind.SizeOfExpression) ||
                   expression is LiteralExpressionSyntax ||
                   expression.IsNameofExpression(semanticModel, cancellationToken) ||
                   expression.IsEnumValueExpression(semanticModel, cancellationToken);
        }
    }
}
