using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

static class CodeAnalysisExtensions
{
	public static INamedTypeSymbol? FindNamedType(
		this IAssemblySymbol assembly,
		Func<INamedTypeSymbol, bool> selector)
	{
		var visitor = new NamedTypeVisitor(selector);
		visitor.Visit(assembly);
		return visitor.MatchingType;
	}

	public static bool IsInTestMethod(
		this IOperation operation,
		XunitContext xunitContext)
	{
		if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null)
			return false;

		var semanticModel = operation.SemanticModel;
		if (semanticModel is null)
			return false;

		for (var parent = operation.Parent; parent != null; parent = parent.Parent)
		{
			if (parent is not IMethodBodyOperation methodBodyOperation)
				continue;
			if (methodBodyOperation.Syntax is not MethodDeclarationSyntax methodSyntax)
				continue;

			return methodSyntax.AttributeLists.SelectMany(list => list.Attributes).Any(attr =>
			{
				var typeInfo = semanticModel.GetTypeInfo(attr);
				if (typeInfo.Type is null)
					return false;

				return
					SymbolEqualityComparer.Default.Equals(typeInfo.Type, xunitContext.Core.FactAttributeType) ||
					SymbolEqualityComparer.Default.Equals(typeInfo.Type, xunitContext.Core.TheoryAttributeType);
			});
		}

		return false;
	}

	public static bool IsTestClass(
		this ITypeSymbol type,
		XunitContext xunitContext)
	{
		var factAttributeType = xunitContext.Core.FactAttributeType;
		var theoryAttributeType = xunitContext.Core.TheoryAttributeType;
		if (factAttributeType is null || theoryAttributeType is null)
			return false;

		var testMethodAttributes =
			new[] { factAttributeType, theoryAttributeType }
				.ToImmutableHashSet(SymbolEqualityComparer.Default);

		return
			type
				.GetMembers()
				.OfType<IMethodSymbol>()
				.Any(method =>
					method
						.GetAttributes()
						.Select(a => a.AttributeClass)
						.Any(t => testMethodAttributes.Contains(t, SymbolEqualityComparer.Default))
				);
	}

	public static IOperation WalkDownImplicitConversions(this IOperation operation)
	{
		var current = operation;
		while (current is IConversionOperation conversion && conversion.Conversion.IsImplicit)
			current = conversion.Operand;

		return current;
	}

	class NamedTypeVisitor : SymbolVisitor
	{
		readonly Func<INamedTypeSymbol, bool> selector;

		public NamedTypeVisitor(Func<INamedTypeSymbol, bool> selector) =>
			this.selector = selector;

		public INamedTypeSymbol? MatchingType { get; private set; }

		public override void VisitAssembly(IAssemblySymbol symbol) =>
			symbol.GlobalNamespace.Accept(this);

		public override void VisitNamespace(INamespaceSymbol symbol)
		{
			if (MatchingType is not null)
				return;

			foreach (var member in symbol.GetMembers())
				member.Accept(this);
		}

		public override void VisitNamedType(INamedTypeSymbol symbol)
		{
			if (MatchingType is not null)
				return;

			if (selector(symbol))
			{
				MatchingType = symbol;
				return;
			}

			foreach (var nestedType in symbol.GetTypeMembers())
				nestedType.Accept(this);
		}
	}
}
