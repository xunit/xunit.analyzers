using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Visits every type in every namespace within an assembly. Expressions are provided which indicate
/// whether the item being searched for has been found. Searches stop once at least one of the
/// short circuit expressions returned true.
/// </summary>
public class SymbolAssemblyVisitor : SymbolVisitor
{
	readonly Func<INamedTypeSymbol, bool>[] shortCircuitExpressions;

	public SymbolAssemblyVisitor(params Func<INamedTypeSymbol, bool>[] shortCircuitExpressions)
	{
		this.shortCircuitExpressions = shortCircuitExpressions;
	}

	public bool ShortCircuitTriggered { get; private set; }

	public override void VisitAssembly(IAssemblySymbol symbol)
	{
		symbol.GlobalNamespace.Accept(this);
	}

	public override void VisitNamespace(INamespaceSymbol symbol)
	{
		var namespaceOrTypes = symbol.GetMembers();
		foreach (var member in namespaceOrTypes)
		{
			if (ShortCircuitTriggered)
				return;

			member.Accept(this);
		}
	}

	public override void VisitNamedType(INamedTypeSymbol symbol)
	{
		if (shortCircuitExpressions.Any(e => e(symbol)))
		{
			ShortCircuitTriggered = true;
			return;
		}

		var nestedTypes = symbol.GetTypeMembers();
		if (nestedTypes.IsDefaultOrEmpty)
			return;

		foreach (var nestedType in nestedTypes)
		{
			if (ShortCircuitTriggered)
				return;

			nestedType.Accept(this);
		}
	}
}
