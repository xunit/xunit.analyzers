using System.Collections;
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
	public class AssertEqualShouldNotBeUsedForCollectionSizeCheck : AssertUsageAnalyzerBase
	{
		static readonly HashSet<string> collectionTypesWithExceptionThrowingGetEnumeratorMethod = new() { "System.ArraySegment<T>" };
		static readonly HashSet<string> sizeMethods = new()
		{
			// Signatures without nullable variants
			"System.Array.Length",
			"System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
			"System.Collections.Immutable.ImmutableArray<T>.Length",
		};
		static readonly string[] targetMethods =
		{
			Constants.Asserts.Equal,
			Constants.Asserts.NotEqual
		};

		public AssertEqualShouldNotBeUsedForCollectionSizeCheck()
			: base(Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck, targetMethods)
		{ }

		protected override void AnalyzeInvocation(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			if (method.Parameters.Length != 2 ||
				!method.Parameters[0].Type.SpecialType.Equals(SpecialType.System_Int32) ||
				!method.Parameters[1].Type.SpecialType.Equals(SpecialType.System_Int32))
				return;

			var sizeOperation = invocationOperation.Arguments.FirstOrDefault(arg => SymbolEqualityComparer.Default.Equals(arg.Parameter, method.Parameters[0]))?.Value;
			var sizeValue = sizeOperation?.ConstantValue ?? default;
			if (!sizeValue.HasValue)
				return;

			// Make sure the first parameter really is an int before checking its value. Could for example be a char.
			if (sizeValue.Value is not int size)
				return;

			if (size < 0 || size > 1 || (size == 1 && method.Name != Constants.Asserts.Equal))
				return;

			var otherArgument = invocationOperation.Arguments.FirstOrDefault(arg => !SymbolEqualityComparer.Default.Equals(arg.Parameter, method.Parameters[0]));

			var symbol = otherArgument?.Value switch
			{
				IInvocationOperation o => o.TargetMethod,
				IPropertyReferenceOperation p => p.Property,
				_ => default(ISymbol),
			};

			if (symbol is null)
				return;

			if (IsCollectionsWithExceptionThrowingGetEnumeratorMethod(symbol) ||
					!IsWellKnownSizeMethod(symbol) &&
					!IsICollectionCountProperty(context, symbol) &&
					!IsICollectionOfTCountProperty(context, symbol) &&
					!IsIReadOnlyCollectionOfTCountProperty(context, symbol))
				return;

			var replacement = GetReplacementMethodName(method.Name, size);

			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.MethodName] = method.Name;
			builder[Constants.Properties.SizeValue] = size.ToString();
			builder[Constants.Properties.Replacement] = replacement;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck,
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

		static string GetReplacementMethodName(
			string methodName,
			int size)
		{
			if (size == 1)
				return Constants.Asserts.Single;

			return methodName == Constants.Asserts.Equal ? Constants.Asserts.Empty : Constants.Asserts.NotEmpty;
		}

		static bool IsCollectionsWithExceptionThrowingGetEnumeratorMethod(ISymbol symbol) =>
			collectionTypesWithExceptionThrowingGetEnumeratorMethod.Contains(symbol.ContainingType.ConstructedFrom.ToDisplayString());

		static bool IsWellKnownSizeMethod(ISymbol symbol) =>
			sizeMethods.Contains(symbol.OriginalDefinition.ToDisplayString());

		static bool IsICollectionCountProperty(
			OperationAnalysisContext context,
			ISymbol symbol) =>
				IsCountPropertyOf(
					context.Compilation.GetTypeByMetadataName(Constants.Types.SystemCollectionsICollection),
					symbol
				);

		static bool IsICollectionOfTCountProperty(
			OperationAnalysisContext context,
			ISymbol symbol) =>
				IsCountPropertyOfGenericType(
					context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T),
					symbol
				);

		static bool IsIReadOnlyCollectionOfTCountProperty(
			OperationAnalysisContext context,
			ISymbol symbol) =>
				IsCountPropertyOfGenericType(
					context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T),
					symbol
				);

		static bool IsCountPropertyOfGenericType(
			INamedTypeSymbol openCollectionType,
			ISymbol symbol)
		{
			var containingType = symbol.ContainingType;
			var concreteCollectionType = containingType.GetGenericInterfaceImplementation(openCollectionType);

			return concreteCollectionType is not null && IsCountPropertyOf(concreteCollectionType, symbol);
		}

		static bool IsCountPropertyOf(
			INamedTypeSymbol? collectionType,
			ISymbol symbol)
		{
			if (collectionType == null)
				return false;

			var memberSymbol = symbol;
			var containingType = memberSymbol.ContainingType;
			var countSymbol = collectionType.GetMember(nameof(ICollection.Count));
			if (countSymbol == null)
				return false;

			var countSymbolImplementation = containingType.FindImplementationForInterfaceMember(countSymbol);

			return SymbolEqualityComparer.Default.Equals(countSymbolImplementation, memberSymbol);
		}
	}
}
