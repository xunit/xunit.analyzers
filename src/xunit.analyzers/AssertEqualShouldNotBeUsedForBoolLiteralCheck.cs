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
		internal static string MethodName = "MethodName";
		internal static string LiteralValue = "LiteralValue";
		internal static readonly HashSet<string> EqualMethods = new HashSet<string>(new[] { "Equal", "StrictEqual" });
		internal static readonly HashSet<string> NotEqualMethods = new HashSet<string>(new[] { "NotEqual", "NotStrictEqual" });

		public AssertEqualShouldNotBeUsedForBoolLiteralCheck()
			: base(Descriptors.X2004_AssertEqualShouldNotUsedForBoolLiteralCheck, EqualMethods.Union(NotEqualMethods))
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 2 && arguments.Length != 3)
				return;

			// Match Assert.Equal<bool>(true, expression) but not e.g. Assert.Equal<object>(true, expression).
			if (!method.IsGenericMethod ||
				!method.TypeArguments[0].SpecialType.Equals(SpecialType.System_Boolean))
				return;

			if (!(arguments.FirstOrDefault(arg => arg.Parameter.Ordinal == 0)?.Value is ILiteralOperation literalFirstArgument))
				return;

			var isTrue = literalFirstArgument.ConstantValue.HasValue && Equals(literalFirstArgument.ConstantValue.Value, true);
			var isFalse = literalFirstArgument.ConstantValue.HasValue && Equals(literalFirstArgument.ConstantValue.Value, false);

			if (!(isTrue ^ isFalse))
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			builder[LiteralValue] = isTrue ? bool.TrueString : bool.FalseString;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2004_AssertEqualShouldNotUsedForBoolLiteralCheck,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
