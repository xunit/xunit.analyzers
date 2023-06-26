using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public static class SymbolExtensions
{
	public static bool ContainsAttributeType(
		this ImmutableArray<AttributeData> attributes,
		INamedTypeSymbol attributeType,
		bool exactMatch = false) =>
			attributes.Any(a => attributeType.IsAssignableFrom(a.AttributeClass, exactMatch));

	/// <summary>
	/// If the passed <paramref name="typeSymbol"/> is <see cref="IEnumerable{T}"/>, then returns
	/// the enumerable type (aka, T); otherwise, returns <c>null</c>.
	/// </summary>
	public static ITypeSymbol? GetEnumerableType(this ITypeSymbol? typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
			return null;

		if (typeSymbol.OriginalDefinition.SpecialType != SpecialType.System_Collections_Generic_IEnumerable_T)
			return null;

		return namedTypeSymbol.TypeArguments[0];
	}

	public static INamedTypeSymbol? GetGenericInterfaceImplementation(
		this ITypeSymbol implementingType,
		INamedTypeSymbol openInterfaceType) =>
			implementingType.AllInterfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, openInterfaceType));

	public static ISymbol? GetMember(
		this INamespaceOrTypeSymbol namespaceOrType,
		string name) =>
			namespaceOrType.GetMembers(name).FirstOrDefault();

	public static ImmutableArray<ISymbol> GetInheritedAndOwnMembers(
		this ITypeSymbol? symbol,
		string? name = null)
	{
		var builder = ImmutableArray.CreateBuilder<ISymbol>();
		while (symbol is not null)
		{
			builder.AddRange(name is null ? symbol.GetMembers() : symbol.GetMembers(name));
			symbol = symbol.BaseType;
		}

		return builder.ToImmutable();
	}

	public static bool IsAssignableFrom(
		this ITypeSymbol? targetType,
		ITypeSymbol? sourceType,
		bool exactMatch = false)
	{
		if (targetType is not null)
		{
			var targetEnumerableType = targetType.GetEnumerableType();

			while (sourceType is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(sourceType, targetType))
					return true;

				if (exactMatch)
					return false;

				// Special handling for IEnumerable<T> == IEnumerable<T>, because the Ts are covariant and the
				// symbol equality comparerer is an exact comparison, not a compatibility test.
				if (targetEnumerableType is not null)
				{
					var sourceEnumerableType = sourceType.GetEnumerableType();
					if (sourceEnumerableType is not null)
						return IsAssignableFrom(targetEnumerableType, sourceEnumerableType);
				}

				if (targetType.TypeKind == TypeKind.Interface)
					return sourceType.AllInterfaces.Any(i => IsAssignableFrom(targetType, i));

				sourceType = sourceType.BaseType;
			}
		}

		return false;
	}
}
