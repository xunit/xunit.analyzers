using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

		protected override void Analyze(OperationAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var arguments = invocation.ArgumentList.Arguments;
			if (arguments.Count < 1 || arguments.Count > 2)
				return;

			if (method.Parameters.Length > 1 && method.Parameters[1].Type.SpecialType.Equals(SpecialType.System_String))
				return;

			if (!(arguments.First().Expression is InvocationExpressionSyntax invocationExpression))
				return;

			var symbolInfo = context.GetSemanticModel().GetSymbolInfo(invocationExpression);
			if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
				return;

			var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
			if (!IsLinqContainsMethod(methodSymbol) && !IsICollectionContainsMethod(context, symbolInfo))
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2017_AssertCollectionContainsShouldNotUseBoolCheck,
					invocation.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}

		private static bool IsLinqContainsMethod(IMethodSymbol methodSymbol)
			=> methodSymbol.ReducedFrom != null && LinqContainsMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol.ReducedFrom));

		private static bool IsICollectionContainsMethod(OperationAnalysisContext context, SymbolInfo symbolInfo)
		{
			var methodSymbol = symbolInfo.Symbol;
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
