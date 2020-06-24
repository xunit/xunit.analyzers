using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEqualPrecisionShouldBeInRange : AssertUsageAnalyzerBase
	{
		public AssertEqualPrecisionShouldBeInRange()
			: base(Descriptors.X2016_AssertEqualPrecisionShouldBeInRange, new[] { "Equal", "NotEqual" })
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation,
			InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var numericType = GetMethodNumericType(method);
			if (numericType == null)
				return;

			var precision = GetNumericLiteralValue(invocationOperation);
			if (precision == null)
				return;

			EnsurePrecisionInRange(context, precision.Value.precisionArgument.Value.Syntax.GetLocation(), numericType.Value, precision.Value.value);
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
		private static (IArgumentOperation precisionArgument, int value)? GetNumericLiteralValue(IInvocationOperation invocation)
		{
			if (invocation.Arguments.Length != 3)
				return null;

			var precisionParameter = invocation.TargetMethod.Parameters[2];
			var precisionArgument = invocation.Arguments.First(arg => arg.Parameter.Equals(precisionParameter));
			if (precisionArgument is null)
				return null;

			var constantValue = precisionArgument.Value.ConstantValue;
			if (!constantValue.HasValue || !(constantValue.Value is int value))
				return null;

			return (precisionArgument, value);
		}

		private static void EnsurePrecisionInRange(OperationAnalysisContext context, Location location,
			SpecialType numericType, int numericValue)
		{
			var precisionMax = PrecisionMaxLimits[numericType];

			if (numericValue < 0 || numericValue > precisionMax)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X2016_AssertEqualPrecisionShouldBeInRange,
						location,
						$"[0..{precisionMax}]",
						TypeNames[numericType]));
			}
		}

		private static readonly IReadOnlyDictionary<SpecialType, int> PrecisionMaxLimits =
			new Dictionary<SpecialType, int>
			{
				{ SpecialType.System_Double, 15 },
				{ SpecialType.System_Decimal, 28 }
			};

		private static readonly IReadOnlyDictionary<SpecialType, string> TypeNames =
			new Dictionary<SpecialType, string>
			{
				{ SpecialType.System_Double, "double" },
				{ SpecialType.System_Decimal, "decimal" }
			};
	}
}
