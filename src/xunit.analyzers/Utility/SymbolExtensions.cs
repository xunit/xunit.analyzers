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
			attributes.Any(a => a.IsInstanceOf(attributeType, exactMatch));

	/// <summary>
	/// If the passed <paramref name="typeSymbol"/> is <see cref="IAsyncEnumerable{T}"/>, then returns
	/// the enumerable type (aka, T); otherwise, returns <c>null</c>.
	/// </summary>
	public static ITypeSymbol? GetAsyncEnumerableType(this ITypeSymbol? typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
			return null;

		// Ideally we'd use symbol comparison here, but that would require threading the
		// Compilation object here, and with 44 callers to IsAssignableFrom that seemed
		// like an unnecessarily daunting task. We cross fingers that this is good. :)
		if (namedTypeSymbol.Name == "IAsyncEnumerable" && namedTypeSymbol.ContainingNamespace.ToString() == "System.Collections.Generic")
			return namedTypeSymbol.TypeArguments[0];

		return null;
	}

	/// <summary>
	/// If the passed <paramref name="typeSymbol"/> is <see cref="IEnumerable{T}"/>, then returns
	/// the enumerable type (aka, T); otherwise, returns <c>null</c>.
	/// </summary>
	public static ITypeSymbol? GetEnumerableType(this ITypeSymbol? typeSymbol)
	{
		if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
			return null;

		if (namedTypeSymbol.OriginalDefinition.SpecialType != SpecialType.System_Collections_Generic_IEnumerable_T)
			return null;

		return namedTypeSymbol.TypeArguments[0];
	}

	public static INamedTypeSymbol? GetGenericInterfaceImplementation(
		this ITypeSymbol implementingType,
		INamedTypeSymbol openInterfaceType)
	{
		Guard.ArgumentNotNull(implementingType);
		Guard.ArgumentNotNull(openInterfaceType);

		if (SymbolEqualityComparer.Default.Equals(implementingType.OriginalDefinition, openInterfaceType))
			return implementingType as INamedTypeSymbol;

		return implementingType.AllInterfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, openInterfaceType));
	}

	public static ISymbol? GetMember(
		this INamespaceOrTypeSymbol namespaceOrType,
		string name) =>
			Guard.ArgumentNotNull(namespaceOrType).GetMembers(name).FirstOrDefault();

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
			var targetAsyncEnumerableType = targetType.GetAsyncEnumerableType();

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

				// Special handling for IAsyncEnumerable<T> == IAsyncEnumerable<T> as well
				if (targetAsyncEnumerableType is not null)
				{
					var sourceAsyncEnumerableType = sourceType.GetAsyncEnumerableType();
					if (sourceAsyncEnumerableType is not null)
						return IsAssignableFrom(targetAsyncEnumerableType, sourceAsyncEnumerableType);
				}

				// Special handling for tuples as tuples with differently named fields are still assignable
				if (targetType.IsTupleType && sourceType.IsTupleType)
				{
					ITypeSymbol targetTupleType = ((INamedTypeSymbol)targetType).TupleUnderlyingType ?? targetType;
					ITypeSymbol sourceTupleType = ((INamedTypeSymbol)sourceType).TupleUnderlyingType ?? sourceType;
					return SymbolEqualityComparer.Default.Equals(sourceTupleType, targetTupleType);
				}

				// Special handling when the target type is an open generic, we need to get the open
				// generic of the source type for the compatibility test
				if (targetType is INamedTypeSymbol namedTargetType && namedTargetType.IsUnboundGenericType)
				{
					var namedSourceType = sourceType as INamedTypeSymbol;
					if (namedSourceType is not null &&
							namedSourceType.IsGenericType &&
							!namedSourceType.IsUnboundGenericType &&
							IsAssignableFrom(targetType, namedSourceType.ConstructUnboundGenericType()))
						return true;
				}

				if (targetType.TypeKind == TypeKind.Interface)
					return sourceType.AllInterfaces.Any(i => IsAssignableFrom(targetType, i));

				sourceType = sourceType.BaseType;
			}
		}

		return false;
	}

	public static bool IsInstanceOf(
		this AttributeData attribute,
		INamedTypeSymbol attributeType,
		bool exactMatch = false) =>
			Guard.ArgumentNotNull(attributeType).IsAssignableFrom(
				Guard.ArgumentNotNull(attribute).AttributeClass,
				exactMatch
			);

	public static ITypeSymbol? UnwrapEnumerable(
		this ITypeSymbol? type,
		Compilation compilation)
	{
		if (type is null)
			return null;

		var iEnumerableOfT = TypeSymbolFactory.IEnumerableOfT(compilation);
		var result = UnwrapEnumerable(type, iEnumerableOfT);

		if (result is null)
		{
			var iAsyncEnumerableOfT = TypeSymbolFactory.IAsyncEnumerableOfT(compilation);
			if (iAsyncEnumerableOfT is not null)
				result = UnwrapEnumerable(type, iAsyncEnumerableOfT);
		}

		return result;
	}

	public static ITypeSymbol? UnwrapEnumerable(
		this ITypeSymbol? type,
		ITypeSymbol enumerableType)
	{
		if (type is null)
			return null;

		IEnumerable<INamedTypeSymbol> interfaces = type.AllInterfaces;
		if (type is INamedTypeSymbol namedType)
			interfaces = interfaces.Concat([namedType]);

		foreach (var @interface in interfaces)
			if (SymbolEqualityComparer.Default.Equals(@interface.OriginalDefinition, enumerableType))
				return @interface.TypeArguments[0];

		return null;
	}

	public static ITypeSymbol UnwrapNullable(this ITypeSymbol type)
	{
		Guard.ArgumentNotNull(type);

		if (type is not INamedTypeSymbol namedType)
			return type;

		if (namedType.IsGenericType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
			return namedType.TypeArguments[0];

		return type;
	}
}
