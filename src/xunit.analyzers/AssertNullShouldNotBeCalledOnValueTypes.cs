using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertNullShouldNotBeCalledOnValueTypes : DiagnosticAnalyzer
    {
        internal const string NullMethod = "Null";
        internal const string NotNullMethod = "NotNull";

        static HashSet<string> methodNames = new HashSet<string>(StringComparer.Ordinal) { NullMethod, NotNullMethod };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
           ImmutableArray.Create(Constants.Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes);

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
                    if (invocation.ArgumentList.Arguments.Count != 1)
                        return;

                    var symbol = syntaxContext.SemanticModel.GetSymbolInfo(invocation, syntaxContext.CancellationToken).Symbol;
                    if (symbol?.Kind != SymbolKind.Method)
                        return;

                    var methodSymbol = (IMethodSymbol)symbol;
                    if (methodSymbol.MethodKind != MethodKind.Ordinary ||
                        methodSymbol.ContainingType != assertType ||
                        !methodNames.Contains(methodSymbol.Name))
                        return;

                    var argumentType = syntaxContext.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments.Single().Expression, syntaxContext.CancellationToken).Type;
                    if (argumentType == null ||
                       argumentType.IsReferenceType ||
                       (argumentType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
                        return;

                    syntaxContext.ReportDiagnostic(Diagnostic.Create(
                        Constants.Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes,
                        invocation.GetLocation(),
                        SymbolDisplay.ToDisplayString(methodSymbol, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None)),
                        SymbolDisplay.ToDisplayString(argumentType, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None))));
                }, SyntaxKind.InvocationExpression);
            });
        }
    }
}
