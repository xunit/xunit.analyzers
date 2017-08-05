using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertEqualGenericShouldNotBeUsedForStringValue : AssertUsageAnalyzerBase
    {
        private static readonly HashSet<string> EqualMethods = new HashSet<string>(new[] { "Equal", "StrictEqual" });

        public AssertEqualGenericShouldNotBeUsedForStringValue() :
            base(Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue, EqualMethods)
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
        {
            if (invocation.ArgumentList.Arguments.Count != 2)
                return;

            if (!method.IsGenericMethod && method.Name == "Equal")
                return;

            if (method.IsGenericMethod &&
               (!method.TypeArguments[0].SpecialType.Equals(SpecialType.System_String) ||
                !method.Parameters[0].Type.SpecialType.Equals(SpecialType.System_String) ||
                !method.Parameters[1].Type.SpecialType.Equals(SpecialType.System_String)))
                return;

            var invalidUsageDescription = method.Name == "Equal" ? "generic Assert.Equal overload" : "Assert.StrictEqual";
            context.ReportDiagnostic(Diagnostic.Create(
                Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue,
                invocation.GetLocation(),
                invalidUsageDescription));
        }
    }
}
