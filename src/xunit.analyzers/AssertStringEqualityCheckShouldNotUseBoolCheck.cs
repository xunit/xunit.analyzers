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
    public class AssertStringEqualityCheckShouldNotUseBoolCheck : AssertUsageAnalyzerBase
    {
        public const string AssertMethodName = "AssertMethodName";
        public const string IsStaticMethodCall = "IsStaticMethodCall";
        public const string IgnoreCase = "IgnoreCase";

        private static readonly HashSet<string> BooleanMethods = new HashSet<string>(new[] { "True", "False" });
        private static readonly HashSet<string> EqualsMethods = new HashSet<string>(new[]
        {
            "string.Equals(string)",
            "string.Equals(string, string)",
            "string.Equals(string, System.StringComparison)",
            "string.Equals(string, string, System.StringComparison)"
        });

        private static readonly HashSet<StringComparison> SupportedStringComparisons = new HashSet<StringComparison>(new[]
        {
            StringComparison.Ordinal,
            StringComparison.OrdinalIgnoreCase
        });

        public AssertStringEqualityCheckShouldNotUseBoolCheck() :
            base(Constants.Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer, BooleanMethods)
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
            if (!EqualsMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
                return;
            
            string ignoreCase = null;

            if (methodSymbol.Parameters.Last().Type.TypeKind == TypeKind.Enum)
            {
                if (method.Name == "False")
                    return;

                var stringComparisonExpression = invocationExpression.ArgumentList.Arguments.Last().Expression;
                var stringComparison = (StringComparison)context.SemanticModel.GetConstantValue(stringComparisonExpression).Value;

                if (!SupportedStringComparisons.Contains(stringComparison))
                    return;

                ignoreCase = stringComparison == StringComparison.OrdinalIgnoreCase ? bool.TrueString : bool.FalseString;
            }

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder[AssertMethodName] = method.Name;
            builder[IsStaticMethodCall] = methodSymbol.IsStatic ? bool.TrueString : bool.FalseString;
            builder[IgnoreCase] = ignoreCase;

            context.ReportDiagnostic(Diagnostic.Create(
                Constants.Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer,
                invocation.GetLocation(),
                builder.ToImmutable(),
                SymbolDisplay.ToDisplayString(method, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
        }
    }
}
