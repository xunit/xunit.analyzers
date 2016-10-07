using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertNullShouldNotBeCalledOnValueTypes : AssertUsageAnalyzerBase
    {
        internal const string NullMethod = "Null";
        internal const string NotNullMethod = "NotNull";

        public AssertNullShouldNotBeCalledOnValueTypes() : base(
            Constants.Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes,
            new[] { NullMethod, NotNullMethod })
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            if (invocation.ArgumentList.Arguments.Count != 1)
                return;

            var argumentType = context.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments.Single().Expression, context.CancellationToken).Type;
            if (argumentType == null ||
               argumentType.IsReferenceType ||
               (argumentType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                Constants.Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes,
                invocation.GetLocation(),
                SymbolDisplay.ToDisplayString(method, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None)),
                SymbolDisplay.ToDisplayString(argumentType, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None))));
        }
    }
}
