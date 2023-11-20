using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Visits every type in every namespace within an assembly
/// </summary>
public class SymbolAssemblyVisitor : SymbolVisitor
{
	private readonly Func<INamedTypeSymbol, bool>[] _shortCircuitExpressions;
	public bool ShortCircuitTriggered { get; private set; }

	public SymbolAssemblyVisitor(params Func<INamedTypeSymbol, bool>[] shortCircuitExpressions)
	{
		_shortCircuitExpressions = shortCircuitExpressions;
	}

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
		if (_shortCircuitExpressions.Any(e => e(symbol)))
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
