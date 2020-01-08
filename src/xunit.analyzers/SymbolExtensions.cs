using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	internal static class SymbolExtensions
	{
		internal static bool ContainsAttributeType(this ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType, bool exactMatch = false)
			=> attributes.Any(a => attributeType.IsAssignableFrom(a.AttributeClass, exactMatch));

		internal static INamedTypeSymbol GetGenericInterfaceImplementation(this ITypeSymbol implementingType, INamedTypeSymbol openInterfaceType)
			=> implementingType.AllInterfaces.FirstOrDefault(i => Equals(i.OriginalDefinition, openInterfaceType));

		internal static ISymbol GetMember(this INamespaceOrTypeSymbol namespaceOrType, string name)
			=> namespaceOrType.GetMembers(name).FirstOrDefault();

		internal static ImmutableArray<ISymbol> GetInheritedAndOwnMembers(this ITypeSymbol symbol, string name = null)
		{
			var builder = ImmutableArray.CreateBuilder<ISymbol>();
			while (symbol != null)
			{
				builder.AddRange(name == null ? symbol.GetMembers() : symbol.GetMembers(name));
				symbol = symbol.BaseType;
			}

			return builder.ToImmutable();
		}

		internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType, bool exactMatch = false)
		{
			if (targetType != null)
			{
				while (sourceType != null)
				{
					if (sourceType.Equals(targetType))
						return true;

					if (exactMatch)
						return false;

					if (targetType.TypeKind == TypeKind.Interface)
						return sourceType.AllInterfaces.Any(i => i.Equals(targetType));

					sourceType = sourceType.BaseType;
				}
			}

			return false;
		}
	}
}
