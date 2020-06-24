using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck : AssertUsageAnalyzerBase
	{
		public const string AssertMethodName = "AssertMethodName";
		private const string EnumerableAnyExtensionMethod = "System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";

		private static readonly HashSet<string> BooleanMethods = new HashSet<string>(new[] { "True", "False" });

		public AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck()
			: base(Descriptors.X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck, BooleanMethods)
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 1)
				return;

			if (!(arguments[0].Value is IInvocationOperation invocationExpression))
				return;

			var methodSymbol = invocationExpression.TargetMethod;
			if (SymbolDisplay.ToDisplayString(methodSymbol.OriginalDefinition) != EnumerableAnyExtensionMethod)
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[AssertMethodName] = method.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
