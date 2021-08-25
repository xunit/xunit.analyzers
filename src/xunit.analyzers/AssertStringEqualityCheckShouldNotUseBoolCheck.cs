using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertStringEqualityCheckShouldNotUseBoolCheck : AssertUsageAnalyzerBase
	{
		static readonly HashSet<string> stringEqualsMethods = new()
		{
			"string.Equals(string)",
			"string.Equals(string, string)",
			"string.Equals(string, System.StringComparison)",
			"string.Equals(string, string, System.StringComparison)"
		};
		static readonly HashSet<StringComparison> supportedStringComparisons = new()
		{
			StringComparison.Ordinal,
			StringComparison.OrdinalIgnoreCase
		};
		static readonly string[] targetMethods =
		{
			Constants.Asserts.True,
			Constants.Asserts.False
		};

		public AssertStringEqualityCheckShouldNotUseBoolCheck()
			: base(Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer, targetMethods)
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 1)
				return;

			if (arguments[0].Value is not IInvocationOperation invocationExpression)
				return;

			var methodSymbol = invocationExpression.TargetMethod;
			if (!stringEqualsMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
				return;

			string ignoreCase = null;

			if (methodSymbol.Parameters.Last().Type.TypeKind == TypeKind.Enum)
			{
				if (method.Name == Constants.Asserts.False)
					return;

				var stringComparisonExpression = invocationExpression.Arguments.FirstOrDefault(arg => arg.Parameter.Equals(methodSymbol.Parameters.Last()))?.Value;
				var stringComparison = (StringComparison?)(int?)(stringComparisonExpression?.ConstantValue.Value);
				if (stringComparison is null)
					return;

				if (!supportedStringComparisons.Contains(stringComparison.Value))
					return;

				ignoreCase = stringComparison == StringComparison.OrdinalIgnoreCase ? bool.TrueString : bool.FalseString;
			}

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[Constants.Properties.AssertMethodName] = method.Name;
			builder[Constants.Properties.IsStaticMethodCall] = methodSymbol.IsStatic ? bool.TrueString : bool.FalseString;
			builder[Constants.Properties.IgnoreCase] = ignoreCase;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat
							.CSharpShortErrorMessageFormat
							.WithParameterOptions(SymbolDisplayParameterOptions.None)
							.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
					)
				)
			);
		}
	}
}
