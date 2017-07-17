using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck : AssertUsageAnalyzerBase
    {
        public const string AssertMethodName = "AssertMethodName";
        private const string EnumerableAnyExtensionMethod = "System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";

        private static readonly HashSet<string> BooleanMethods = new HashSet<string>(new[] { "True", "False" });
        
        public AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck() :
            base(Descriptors.X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck, BooleanMethods)
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count != 1)
                return;

            var invocationExpression = arguments.First().Expression as InvocationExpressionSyntax;
            if (invocationExpression == null)
                return;

            var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpression);
            if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
                return;

            var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
            if (methodSymbol.ReducedFrom == null || SymbolDisplay.ToDisplayString(methodSymbol.ReducedFrom) != EnumerableAnyExtensionMethod)
                return;

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder[AssertMethodName] = method.Name;
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck,
                invocation.GetLocation(),
                builder.ToImmutable(),
                SymbolDisplay.ToDisplayString(method, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
        }
    }
}