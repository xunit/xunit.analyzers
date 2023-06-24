using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEqualPrecisionShouldBeInRange : AssertUsageAnalyzerBase
{
	static readonly Dictionary<SpecialType, int> precisionMaxLimits = new()
	{
		{ SpecialType.System_Double, 15 },
		{ SpecialType.System_Decimal, 28 },
	};
	static readonly string[] targetMethods =
	{
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
	};
	static readonly Dictionary<SpecialType, string> typeNames = new()
	{
		{ SpecialType.System_Double, "double" },
		{ SpecialType.System_Decimal, "decimal" },
	};

	public AssertEqualPrecisionShouldBeInRange()
		: base(Descriptors.X2016_AssertEqualPrecisionShouldBeInRange, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		var numericType = GetMethodNumericType(method);
		if (numericType is null)
			return;

		var precision = GetNumericLiteralValue(invocationOperation);
		if (precision is null)
			return;

		EnsurePrecisionInRange(context, precision.Value.precisionArgument.Value.Syntax.GetLocation(), numericType.Value, precision.Value.value);
	}

	// Gets double or decimal SpecialType if it is method's first argument type, null otherwise.
	// This has the semantic of: 1. Ensure the analysis applies, 2. Get data for further analysis.
	static SpecialType? GetMethodNumericType(IMethodSymbol method)
	{
		if (method.Parameters.Length != 3)
			return null;

		var type = method.Parameters[0].Type.SpecialType;

		if (type != SpecialType.System_Double && type != SpecialType.System_Decimal)
			return null;

		return type;
	}

	// Gets the value of precision used in Equal/NotEqual invocation or null if cannot be obtained.
	// This has the semantic of: 1. Ensure the analysis applies, 2. Get data for further analysis.
	static (IArgumentOperation precisionArgument, int value)? GetNumericLiteralValue(IInvocationOperation invocation)
	{
		if (invocation.Arguments.Length != 3)
			return null;

		var precisionParameter = invocation.TargetMethod.Parameters[2];
		var precisionArgument = invocation.Arguments.FirstOrDefault(arg => SymbolEqualityComparer.Default.Equals(arg.Parameter, precisionParameter));
		if (precisionArgument is null)
			return null;

		var constantValue = precisionArgument.Value.ConstantValue;
		if (!constantValue.HasValue || constantValue.Value is not int value)
			return null;

		return (precisionArgument, value);
	}

	static void EnsurePrecisionInRange(
		OperationAnalysisContext context,
		Location location,
		SpecialType numericType,
		int numericValue)
	{
		var precisionMax = precisionMaxLimits[numericType];

		if (numericValue < 0 || numericValue > precisionMax)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.Replacement] = numericValue < 0 ? "0" : precisionMax.ToString();

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2016_AssertEqualPrecisionShouldBeInRange,
					location,
					builder.ToImmutable(),
					$"[0..{precisionMax}]",
					typeNames[numericType]
				)
			);
		}
	}
}
