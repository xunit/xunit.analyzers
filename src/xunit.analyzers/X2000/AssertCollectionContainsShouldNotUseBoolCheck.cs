using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertCollectionContainsShouldNotUseBoolCheck : AssertUsageAnalyzerBase
{
	static readonly HashSet<string> linqContainsMethods = new()
	{
		// Signatures without nullable variants
		"System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource)",
		// Non-nullable signatures
		"System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource, System.Collections.Generic.IEqualityComparer<TSource>)",
		// Nullable signatures
		"System.Linq.Enumerable.Contains<TSource>(System.Collections.Generic.IEnumerable<TSource>, TSource, System.Collections.Generic.IEqualityComparer<TSource>?)",
	};
	static readonly string[] targetMethods =
	{
		Constants.Asserts.False,
		Constants.Asserts.True,
	};

	public AssertCollectionContainsShouldNotUseBoolCheck()
		: base(Descriptors.X2017_AssertCollectionContainsShouldNotUseBoolCheck, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		var arguments = invocationOperation.Arguments;
		if (arguments.Length < 1 || arguments.Length > 2)
			return;

		if (method.Parameters.Length > 1 && method.Parameters[1].Type.SpecialType == SpecialType.System_String)
			return;

		if (arguments.FirstOrDefault(arg => SymbolEqualityComparer.Default.Equals(arg.Parameter, method.Parameters[0]))?.Value is not IInvocationOperation invocationExpression)
			return;

		var methodSymbol = invocationExpression.TargetMethod;
		if (!IsLinqContainsMethod(methodSymbol) && !IsICollectionContainsMethod(context, methodSymbol))
			return;

		var replacement =
			method.Name == Constants.Asserts.True
				? Constants.Asserts.Contains
				: Constants.Asserts.DoesNotContain;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		builder[Constants.Properties.Replacement] = replacement;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2017_AssertCollectionContainsShouldNotUseBoolCheck,
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

	static bool IsLinqContainsMethod(IMethodSymbol methodSymbol) =>
		methodSymbol.OriginalDefinition is not null && linqContainsMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol.OriginalDefinition));

	static bool IsICollectionContainsMethod(
		OperationAnalysisContext context,
		IMethodSymbol methodSymbol)
	{
		var containingType = methodSymbol.ContainingType;
		var genericCollectionType = containingType.GetGenericInterfaceImplementation(context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T));

		if (genericCollectionType is null)
			return false;

		var genericCollectionContainsSymbol =
			genericCollectionType
				.GetMembers(nameof(ICollection<int>.Contains))
				.FirstOrDefault();

		if (genericCollectionContainsSymbol is null)
			return false;

		var genericCollectionSymbolImplementation = containingType.FindImplementationForInterfaceMember(genericCollectionContainsSymbol);
		return SymbolEqualityComparer.Default.Equals(genericCollectionSymbolImplementation, methodSymbol);
	}
}
