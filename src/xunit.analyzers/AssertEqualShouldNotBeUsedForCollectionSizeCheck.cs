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
		internal static string MethodName = "MethodName";
		internal static string SizeValue = "SizeValue";

		private static readonly HashSet<string> CollectionTypesWithExceptionThrowingGetEnumeratorMethod = new HashSet<string>
		{
			"System.ArraySegment<T>"
		};

		private static readonly HashSet<string> EqualMethods = new HashSet<string>(new[] { "Equal", "NotEqual" });

		private static readonly HashSet<string> SizeMethods = new HashSet<string>
		{
			"System.Array.Length",
			"System.Linq.Enumerable.Count<TSource>(System.Collections.Generic.IEnumerable<TSource>)",
			"System.Collections.Immutable.ImmutableArray<T>.Length",
		};

		public AssertEqualShouldNotBeUsedForCollectionSizeCheck()
			: base(Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck, EqualMethods)
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			if (method.Parameters.Length != 2 ||
				!method.Parameters[0].Type.SpecialType.Equals(SpecialType.System_Int32) ||
				!method.Parameters[1].Type.SpecialType.Equals(SpecialType.System_Int32))
				return;

			var sizeOperation = invocationOperation.Arguments.FirstOrDefault(arg => arg.Parameter.Equals(method.Parameters[0]))?.Value;
			var sizeValue = sizeOperation?.ConstantValue ?? default;
			if (!sizeValue.HasValue)
				return;

			// Make sure the first parameter really is an int before checking its value. Could for example be a char.
			if (!(sizeValue.Value is int size))
				return;

			if (size < 0 || size > 1 || size == 1 && method.Name != "Equal")
				return;

			var otherArgument = invocationOperation.Arguments.FirstOrDefault(arg => !arg.Parameter.Equals(method.Parameters[0]));

			ISymbol symbol = otherArgument?.Value switch
			{
				IInvocationOperation o => o.TargetMethod,
				IPropertyReferenceOperation p => p.Property,
				_ => null,
			};

			if (symbol == null)
				return;

			if (IsCollectionsWithExceptionThrowingGetEnumeratorMethod(symbol) ||
				!IsWellKnownSizeMethod(symbol) &&
				!IsICollectionCountProperty(context, symbol) &&
				!IsICollectionOfTCountProperty(context, symbol) &&
				!IsIReadOnlyCollectionOfTCountProperty(context, symbol))
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			builder[SizeValue] = size.ToString();

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}

		private static bool IsCollectionsWithExceptionThrowingGetEnumeratorMethod(ISymbol symbol)
		{
			return CollectionTypesWithExceptionThrowingGetEnumeratorMethod.Contains(symbol.ContainingType.ConstructedFrom.ToDisplayString());
		}

		private static bool IsWellKnownSizeMethod(ISymbol symbol)
			 => SizeMethods.Contains(symbol.OriginalDefinition.ToDisplayString());

		private static bool IsICollectionCountProperty(OperationAnalysisContext context, ISymbol symbol)
			=> IsCountPropertyOf(
				context.Compilation.GetTypeByMetadataName(Constants.Types.SystemCollectionsICollection),
				symbol);

		private static bool IsICollectionOfTCountProperty(OperationAnalysisContext context, ISymbol symbol)
			=> IsCountPropertyOfGenericType(
				context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T),
				symbol);

		private static bool IsIReadOnlyCollectionOfTCountProperty(OperationAnalysisContext context, ISymbol symbol)
			=> IsCountPropertyOfGenericType(
				context.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T),
				symbol);

		private static bool IsCountPropertyOfGenericType(INamedTypeSymbol openCollectionType, ISymbol symbol)
		{
			var containingType = symbol.ContainingType;
			var concreteCollectionType = containingType.GetGenericInterfaceImplementation(openCollectionType);
			return concreteCollectionType != null && IsCountPropertyOf(concreteCollectionType, symbol);
		}

		private static bool IsCountPropertyOf(INamedTypeSymbol collectionType, ISymbol symbol)
		{
			var memberSymbol = symbol;
			var containingType = memberSymbol.ContainingType;
			var countSymbol = collectionType.GetMember("Count");
			var countSymbolImplementation = containingType.FindImplementationForInterfaceMember(countSymbol);
			return countSymbolImplementation?.Equals(memberSymbol) ?? false;
		}
	}
}
