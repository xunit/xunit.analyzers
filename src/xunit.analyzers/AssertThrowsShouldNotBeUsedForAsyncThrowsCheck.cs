using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheck : AssertUsageAnalyzerBase
    {
        internal const string MethodName = "MethodName";

        private static readonly HashSet<string> ObsoleteMethods = new HashSet<string>
        {
            "Xunit.Assert.Throws<System.NotImplementedException>(System.Func<System.Threading.Tasks.Task>)",
            "Xunit.Assert.Throws<System.ArgumentException>(string, System.Func<System.Threading.Tasks.Task>)"
        };

        public AssertThrowsShouldNotBeUsedForAsyncThrowsCheck() :
            base(Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck, new[] { "Throws", "ThrowsAny" })
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            if (invocation.ArgumentList.Arguments.Count < 1 || invocation.ArgumentList.Arguments.Count > 2)
                return;

            var throwExpressionSymbol = GetThrowExpressionSymbol(context, invocation);
            if (!ThrowExpressionReturnsTask(throwExpressionSymbol, context))
                return;

            var descriptor = ObsoleteMethods.Contains(SymbolDisplay.ToDisplayString(method))
                ? Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck_Hidden
                : Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck;

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder[MethodName] = method.Name;
            var diagnostic = Diagnostic.Create(
                descriptor,
                invocation.GetLocation(),
                builder.ToImmutable(),
                SymbolDisplay.ToDisplayString(method, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None)));

            context.ReportDiagnostic(diagnostic);
        }

        private static SymbolInfo GetThrowExpressionSymbol(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation)
        {
            var argumentExpression = invocation.ArgumentList.Arguments.Last().Expression;
            var lambdaExpression = argumentExpression as LambdaExpressionSyntax;

            if (lambdaExpression == null)
                return context.SemanticModel.GetSymbolInfo(argumentExpression);

            var awaitExpression = lambdaExpression.Body as AwaitExpressionSyntax;
            if (awaitExpression == null)
                return context.SemanticModel.GetSymbolInfo(lambdaExpression.Body);

            return context.SemanticModel.GetSymbolInfo(awaitExpression.Expression);
        }

        private static bool ThrowExpressionReturnsTask(SymbolInfo symbol, SyntaxNodeAnalysisContext context)
        {
            if (symbol.Symbol == null || symbol.Symbol.Kind != SymbolKind.Method)
                return false;

            var taskType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Task).FullName);
            return taskType.IsAssignableFrom(((IMethodSymbol)symbol.Symbol).ReturnType);
        }
    }
}