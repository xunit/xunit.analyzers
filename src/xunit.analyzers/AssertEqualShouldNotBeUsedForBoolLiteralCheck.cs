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
	public class AssertEqualShouldNotBeUsedForBoolLiteralCheck : AssertUsageAnalyzerBase
	{
		static readonly HashSet<string> equalMethods = new()
		{
			Constants.Asserts.Equal,
			Constants.Asserts.StrictEqual
		};
		static readonly HashSet<string> notEqualMethods = new()
		{
			Constants.Asserts.NotEqual,
			Constants.Asserts.NotStrictEqual
		};
		static readonly string[] targetMethods = equalMethods.Union(notEqualMethods).ToArray();

		public AssertEqualShouldNotBeUsedForBoolLiteralCheck()
			: base(Descriptors.X2004_AssertEqualShouldNotUsedForBoolLiteralCheck, targetMethods)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 2 && arguments.Length != 3)
				return;

			// Match Assert.Equal<bool>(true, expression) but not e.g. Assert.Equal<object>(true, expression).
			if (!method.IsGenericMethod ||
				!method.TypeArguments[0].SpecialType.Equals(SpecialType.System_Boolean))
				return;

			if (arguments.FirstOrDefault(arg => arg.Parameter?.Ordinal == 0)?.Value is not ILiteralOperation literalFirstArgument)
				return;

			var isTrue = literalFirstArgument.ConstantValue.HasValue && Equals(literalFirstArgument.ConstantValue.Value, true);
			var isFalse = literalFirstArgument.ConstantValue.HasValue && Equals(literalFirstArgument.ConstantValue.Value, false);

			if (!(isTrue ^ isFalse))
				return;

			var replacement = GetReplacementMethodName(method.Name, isTrue);
			if (replacement is null)
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.MethodName] = method.Name;
			builder[Constants.Properties.LiteralValue] = isTrue ? bool.TrueString : bool.FalseString;
			builder[Constants.Properties.Replacement] = replacement;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2004_AssertEqualShouldNotUsedForBoolLiteralCheck,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat
							.CSharpShortErrorMessageFormat
							.WithParameterOptions(SymbolDisplayParameterOptions.None)
							.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
					),
					replacement
				)
			);
		}

		static string? GetReplacementMethodName(
			string methodName,
			bool isTrue)
		{
			if (equalMethods.Contains(methodName))
				return isTrue ? Constants.Asserts.True : Constants.Asserts.False;
			if (notEqualMethods.Contains(methodName))
				return isTrue ? Constants.Asserts.False : Constants.Asserts.True;

			return null;
		}
	}
}
