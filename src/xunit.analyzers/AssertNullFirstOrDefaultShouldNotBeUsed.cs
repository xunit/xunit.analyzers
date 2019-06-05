using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertNullFirstOrDefaultShouldNotBeUsed : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptors.X2020_AssertNullFirstOrDefaultShouldNotBeUsed);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(context =>
            {
                var invocationExpression = (InvocationExpressionSyntax)context.Node;
                var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;

                var calledMethodName = memberAccessExpression?.Name.ToString();

                if (calledMethodName != "Null" && calledMethodName != "NotNull")
                    return;

                var memberSymbol = context
                    .SemanticModel
                    .GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

                if (!memberSymbol?.ToString().StartsWith("Xunit.Assert") ?? true)
                    return;

                var argumentList = invocationExpression.ArgumentList;

                if (argumentList?.Arguments.Count < 1)
                    return;

                if (argumentList.Arguments[0].Expression is InvocationExpressionSyntax argumentInvocationExpression &&
                    argumentInvocationExpression.Expression is MemberAccessExpressionSyntax argumentMemberAccessExpression &&
                    argumentMemberAccessExpression.Name.ToString() == "FirstOrDefault")
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X2020_AssertNullFirstOrDefaultShouldNotBeUsed,
                        invocationExpression.GetLocation()));
                }

            }, ImmutableArray.Create(SyntaxKind.InvocationExpression));
        }
    }
}
