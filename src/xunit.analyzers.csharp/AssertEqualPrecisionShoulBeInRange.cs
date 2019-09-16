using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertEqualPrecisionShouldBeInRange : AssertUsageAnalyzerBase
    {
        public AssertEqualPrecisionShouldBeInRange()
            : base(Descriptors.X2016_AssertEqualPrecisionShouldBeInRange, new[] { "Equal", "NotEqual" })
        {
        }

        protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation,
            IMethodSymbol method)
        {
            var numericType = GetMethodNumericType(method);
            if (numericType == null)
                return;

            var numericLiteralValue = GetNumericLiteralValue(invocation, context.SemanticModel);
            if (numericLiteralValue == null)
                return;

            var precisionLiteralExpression = invocation.ArgumentList.Arguments[2].Expression;

            EnsurePrecisionInRange(context, precisionLiteralExpression.GetLocation(), numericType.Value,
                numericLiteralValue.Value);
        }

        /// <summary>
        /// Gets double or decimal <see cref="SpecialType"/> if it is method's first argument type, <c>null</c> otherwise.
        /// This has the semantic of: 1. Ensure the analysis applies, 2. Get data for further analysis.
        /// </summary>
        private static SpecialType? GetMethodNumericType(IMethodSymbol method)
        {
            if (method.Parameters.Length != 3)
                return null;

            var type = method.Parameters[0].Type.SpecialType;

            if (type != SpecialType.System_Double && type != SpecialType.System_Decimal)
                return null;

            return type;
        }

        /// <summary>
        /// Gets the value of precision used in Equal/NotEqual invocation or <c>null</c> if cannot be obtained.
        /// This has the semantic of: 1. Ensure the analysis applies, 2. Get data for further analysis.
        /// </summary>
        /// <param name="invocation"></param>
        /// <param name="contextSemanticModel"></param>
        private static int? GetNumericLiteralValue(InvocationExpressionSyntax invocation,
            SemanticModel contextSemanticModel)
        {
            if (invocation.ArgumentList?.Arguments.Count != 3)
                return null;

            var precisionArgumentExpression = invocation.ArgumentList.Arguments[2].Expression;
            var constantValue = contextSemanticModel.GetConstantValue(precisionArgumentExpression);

            return constantValue.HasValue && constantValue.Value is int
                ? (int?)constantValue.Value
                : null;
        }

        private static void EnsurePrecisionInRange(SyntaxNodeAnalysisContext context, Location location,
            SpecialType numericType, int numericValue)
        {
            var precisionMax = PrecisionMaxLimits[numericType];

            if (numericValue < 0 || numericValue > precisionMax)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.X2016_AssertEqualPrecisionShouldBeInRange,
                    location, $"[0..{precisionMax}]", TypeNames[numericType]));
            }
        }

        private static readonly IReadOnlyDictionary<SpecialType, int> PrecisionMaxLimits =
            new Dictionary<SpecialType, int>
            {
                {SpecialType.System_Double, 15},
                {SpecialType.System_Decimal, 28}
            };

        private static readonly IReadOnlyDictionary<SpecialType, string> TypeNames =
            new Dictionary<SpecialType, string>
            {
                {SpecialType.System_Double, "double"},
                {SpecialType.System_Decimal, "decimal"}
            };
    }
}
