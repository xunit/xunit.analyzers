using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertEqualsShouldNotBeUsed : DiagnosticAnalyzer
    {
        internal static string MethodName = "MethodName";
        internal const string EqualsMethod = "Equals";
        internal const string ReferenceEqualsMethod = "ReferenceEquals";

        static HashSet<string> methodNames = new HashSet<string>(StringComparer.Ordinal) { EqualsMethod, ReferenceEqualsMethod };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X2001_AssertEqualsShouldNotBeUsed);

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
                    var symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocation, syntaxContext.CancellationToken).Symbol;
                    if (symbol?.Kind != SymbolKind.Method)
                        return;

                    var methodSymbol = (IMethodSymbol)symbol;
                    if (methodSymbol.MethodKind != MethodKind.Ordinary ||
                        methodSymbol.ContainingType != assertType ||
                        !methodNames.Contains(methodSymbol.Name))
                        return;

                    var builder = ImmutableDictionary.CreateBuilder<string, string>();
                    builder[MethodName] = methodSymbol.Name;
                    syntaxContext.ReportDiagnostic(Diagnostic.Create(
                        Constants.Descriptors.X2001_AssertEqualsShouldNotBeUsed,
                        invocation.GetLocation(),
                        builder.ToImmutable(),
                        SymbolDisplay.ToDisplayString(methodSymbol, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None))));
                }, SyntaxKind.InvocationExpression);
            });
        }
    }
}
