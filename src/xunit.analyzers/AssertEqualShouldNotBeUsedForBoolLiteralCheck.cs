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
    public class AssertEqualShouldNotBeUsedForBoolLiteralCheck : AssertUsageAnalyzerBase
    {
        internal static string MethodName = "MethodName";
        internal static string LiteralValue = "LiteralValue";
        internal static readonly HashSet<string> EqualMethods = new HashSet<string>(new[] { "Equal", "StrictEqual" });
        internal static readonly HashSet<string> NotEqualMethods = new HashSet<string>(new[] { "NotEqual", "NotStrictEqual" });

        public AssertEqualShouldNotBeUsedForBoolLiteralCheck() :
            base(Descriptors.X2004_AssertEqualShouldNotUsedForBoolLiteralCheck, EqualMethods.Union(NotEqualMethods))
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            var arguments = invocation.ArgumentList.Arguments;
            if (arguments.Count != 2 && arguments.Count != 3)
                return;

            var literalFirstArgument = arguments.First().Expression as LiteralExpressionSyntax;
            if (literalFirstArgument == null)
                return;

            var isTrue = literalFirstArgument.IsKind(SyntaxKind.TrueLiteralExpression);
            var isFalse = literalFirstArgument.IsKind(SyntaxKind.FalseLiteralExpression);

            if (!(isTrue ^ isFalse))
                return;

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder[MethodName] = method.Name;
            builder[LiteralValue] = isTrue ? bool.TrueString : bool.FalseString;
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.X2004_AssertEqualShouldNotUsedForBoolLiteralCheck,
                invocation.GetLocation(),
                builder.ToImmutable(),
                SymbolDisplay.ToDisplayString(method, SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
        }
    }
}
