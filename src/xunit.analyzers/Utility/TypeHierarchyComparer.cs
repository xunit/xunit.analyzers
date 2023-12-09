using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class TypeHierarchyComparer : IComparer<ITypeSymbol>
{
	TypeHierarchyComparer()
	{ }

	public static TypeHierarchyComparer Instance { get; } = new TypeHierarchyComparer();

	public int Compare(
		ITypeSymbol? x,
		ITypeSymbol? y)
	{
		Guard.ArgumentValid("The argument must be a class", x?.TypeKind == TypeKind.Class, nameof(x));
		Guard.ArgumentValid("The argument must be a class", y?.TypeKind == TypeKind.Class, nameof(x));

		if (SymbolEqualityComparer.Default.Equals(x, y))
			return 0;
		if (x.IsAssignableFrom(y))
			return -1;
		if (y.IsAssignableFrom(x))
			return 1;

		throw new InvalidOperationException("Encountered types not in a hierarchy");
	}
}
