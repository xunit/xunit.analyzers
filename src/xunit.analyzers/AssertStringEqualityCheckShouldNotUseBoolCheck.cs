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

		public AssertStringEqualityCheckShouldNotUseBoolCheck()
			: base(Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer, BooleanMethods)
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 1)
				return;

			if (!(arguments[0].Value is IInvocationOperation invocationExpression))
				return;

			var methodSymbol = invocationExpression.TargetMethod;
			if (!EqualsMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
				return;

			string ignoreCase = null;

			if (methodSymbol.Parameters.Last().Type.TypeKind == TypeKind.Enum)
			{
				if (method.Name == "False")
					return;

				var stringComparisonExpression = invocationExpression.Arguments.FirstOrDefault(arg => arg.Parameter.Equals(methodSymbol.Parameters.Last()))?.Value;
				var stringComparison = (StringComparison?)(int?)(stringComparisonExpression?.ConstantValue.Value);
				if (stringComparison is null)
					return;

				if (!SupportedStringComparisons.Contains(stringComparison.Value))
					return;

				ignoreCase = stringComparison == StringComparison.OrdinalIgnoreCase ? bool.TrueString : bool.FalseString;
			}

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[AssertMethodName] = method.Name;
			builder[IsStaticMethodCall] = methodSymbol.IsStatic ? bool.TrueString : bool.FalseString;
			builder[IgnoreCase] = ignoreCase;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
