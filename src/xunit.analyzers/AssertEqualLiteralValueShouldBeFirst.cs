using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertEqualLiteralValueShouldBeFirst : AssertUsageAnalyzerBase
    {
        public AssertEqualLiteralValueShouldBeFirst() :
            base(
                Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst,
                new[] { "Equal", "StrictEqual", "NotEqual", "NotStrictEqual" })
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            if (invocation.ArgumentList.Arguments.Count < 2)
                return;

            var firstArg = invocation.ArgumentList.Arguments[0];
            var secondArg = invocation.ArgumentList.Arguments[1];

            if (IsLiteralOrConstant(secondArg.Expression, context.SemanticModel, context.CancellationToken) &&
                !IsLiteralOrConstant(firstArg.Expression, context.SemanticModel, context.CancellationToken))
            {
                var parentMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                var parentType = parentMethod.FirstAncestorOrSelf<ClassDeclarationSyntax>();
                context.ReportDiagnostic(Diagnostic.Create(
                    Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst,
                    invocation.GetLocation(),
                    secondArg.Expression.ToString(),
                    SymbolDisplay.ToDisplayString(method, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None).WithParameterOptions(SymbolDisplayParameterOptions.IncludeName)),
                    parentMethod.Identifier.ValueText,
                    parentType.Identifier.ValueText));
            }
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
