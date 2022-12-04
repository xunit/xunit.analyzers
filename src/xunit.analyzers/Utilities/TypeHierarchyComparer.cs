using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class TypeHierarchyComparer : IComparer<ITypeSymbol>
	{
		TypeHierarchyComparer()
		{ }

		public static TypeHierarchyComparer Instance { get; } = new TypeHierarchyComparer();

		public int Compare(
			ITypeSymbol? x,
			ITypeSymbol? y)
		{
			if (x?.TypeKind != TypeKind.Class)
				throw new ArgumentException("The argument must be a class", nameof(x));
			if (y?.TypeKind != TypeKind.Class)
				throw new ArgumentException("The argument must be a class", nameof(y));

			if (x.Equals(y))
				return 0;
			if (x.IsAssignableFrom(y))
				return -1;
			if (y.IsAssignableFrom(x))
				return 1;

			throw new InvalidOperationException("Encountered types not in a hierarchy");
		}
	}
}
