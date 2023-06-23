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
			while (sourceType is not null)
			{
				if (SymbolEqualityComparer.Default.Equals(sourceType, targetType))
					return true;

				if (exactMatch)
					return false;

				if (targetType.TypeKind == TypeKind.Interface)
					return sourceType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, targetType));

				sourceType = sourceType.BaseType;
			}
		}

		return false;
	}
}
