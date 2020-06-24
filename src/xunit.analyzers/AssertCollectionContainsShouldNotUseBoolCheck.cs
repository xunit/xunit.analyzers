using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertCollectionContainsShouldNotUseBoolCheck : AssertUsageAnalyzerBase
	{
		internal static string MethodName = "MethodName";

		private static readonly HashSet<string> LinqContainsMethods = new HashSet<string>
		{
			"System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)",
			"System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource, System.Collections.Generic.IEqualityComparer<TSource>)"
		};

		public AssertCollectionContainsShouldNotUseBoolCheck()
			: base(Descriptors.X2017_AssertCollectionContainsShouldNotUseBoolCheck, new[] { "True", "False" })
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length < 1 || arguments.Length > 2)
				return;

			if (method.Parameters.Length > 1 && method.Parameters[1].Type.SpecialType == SpecialType.System_String)
				return;

			if (!(arguments.FirstOrDefault(arg => arg.Parameter.Equals(method.Parameters[0]))?.Value is IInvocationOperation invocationExpression))
				return;

			var methodSymbol = invocationExpression.TargetMethod;
			if (!IsLinqContainsMethod(methodSymbol) && !IsICollectionContainsMethod(context, methodSymbol))
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2017_AssertCollectionContainsShouldNotUseBoolCheck,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}

		private static bool IsLinqContainsMethod(IMethodSymbol methodSymbol)
			=> methodSymbol.OriginalDefinition != null && LinqContainsMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol.OriginalDefinition));

		private static bool IsICollectionContainsMethod(OperationAnalysisContext context, IMethodSymbol methodSymbol)
		{
			var containingType = methodSymbol.ContainingType;
			var genericCollectionType = containingType.GetGenericInterfaceImplementation(context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T));

			if (genericCollectionType == null)
				return false;

			var genericCollectionContainsSymbol = genericCollectionType
				.GetMembers(nameof(ICollection<int>.Contains))
				.FirstOrDefault();

			if (genericCollectionContainsSymbol == null)
				return false;

			var genericCollectionSymbolImplementation = containingType.FindImplementationForInterfaceMember(genericCollectionContainsSymbol);
			return genericCollectionSymbolImplementation?.Equals(methodSymbol) ?? false;
		}
	}
}
