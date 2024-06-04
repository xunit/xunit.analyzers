using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
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
		Guard.ArgumentNotNull(assembly);
		Guard.ArgumentNotNull(selector);

		var visitor = new NamedTypeVisitor(selector);
		visitor.Visit(assembly);
		return visitor.MatchingType;
	}

	public static ImmutableArray<AttributeData> GetAttributesWithInheritance(
		this IMethodSymbol method,
		ITypeSymbol? attributeUsageType)
	{
#pragma warning disable RS1024 // This is correct usage
		var result = new Dictionary<INamedTypeSymbol, List<AttributeData>>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024

		foreach (var attribute in method.GetAttributes())
			if (attribute.AttributeClass is not null)
				result.Add(attribute.AttributeClass, attribute);

		if (method.IsOverride && attributeUsageType is not null)
			for (var baseMethod = method.OverriddenMethod; baseMethod != null; baseMethod = baseMethod.OverriddenMethod)
				foreach (var attribute in baseMethod.GetAttributes())
				{
					if (attribute.AttributeClass is null || result.ContainsKey(attribute.AttributeClass))
						continue;

					var inherited = true;
					var allowMultiple = false;

					var usageAttribute = attribute.AttributeClass.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeUsageType));
					if (usageAttribute is not null)
					{
						var inheritedNamedArgument =
							usageAttribute
								.NamedArguments
								.FirstOrDefault(n => n.Key == nameof(AttributeUsageAttribute.Inherited));

						if (inheritedNamedArgument.Value.Value is not null)
							inherited = (bool)inheritedNamedArgument.Value.Value;

						var allowMultipleNamedArgument =
							usageAttribute
								.NamedArguments
								.FirstOrDefault(n => n.Key == nameof(AttributeUsageAttribute.AllowMultiple));

						if (allowMultipleNamedArgument.Value.Value is not null)
							allowMultiple = (bool)allowMultipleNamedArgument.Value.Value;
					}

					if ((allowMultiple || !result.ContainsKey(attribute.AttributeClass)) && inherited)
						result.Add(attribute.AttributeClass, attribute);
				}

		return result.Values.SelectMany(x => x).ToImmutableArray();
	}

	public static bool IsInTestMethod(
		this IOperation operation,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(operation);
		Guard.ArgumentNotNull(xunitContext);

		if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null)
			return false;

		var semanticModel = operation.SemanticModel;
		if (semanticModel is null)
			return false;

		for (var parent = operation.Parent; parent is not null; parent = parent.Parent)
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
		this ITypeSymbol? type,
		XunitContext xunitContext,
		bool strict)
	{
		Guard.ArgumentNotNull(xunitContext);

		if (type is null)
			return false;

		if (strict)
			return IsTestClassStrict(type, xunitContext);
		else
			return IsTestClassNonStrict(type, xunitContext);
	}

	static bool IsTestClassNonStrict(
		ITypeSymbol type,
		XunitContext xunitContext)
	{
		var factAttributeType = xunitContext.Core.FactAttributeType;
		if (factAttributeType is null)
			return false;

		return
			type
				.GetMembers()
				.OfType<IMethodSymbol>()
				.Any(method =>
					method
						.GetAttributes()
						.Select(a => a.AttributeClass)
						.Any(t => factAttributeType.IsAssignableFrom(t))
				);
	}

	static bool IsTestClassStrict(
		ITypeSymbol type,
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

	public static bool IsTestMethod(
		this IMethodSymbol method,
		XunitContext xunitContext,
		ITypeSymbol attributeUsageType,
		bool strict)
	{
		Guard.ArgumentNotNull(method);
		Guard.ArgumentNotNull(xunitContext);

		var factAttributeType = xunitContext.Core.FactAttributeType;
		var theoryAttributeType = xunitContext.Core.TheoryAttributeType;
		if (factAttributeType is null || theoryAttributeType is null)
			return false;

		var attributes = method.GetAttributesWithInheritance(attributeUsageType);
		var comparer = SymbolEqualityComparer.Default;

		return
			strict
				? attributes.Any(a => comparer.Equals(a.AttributeClass, factAttributeType) || comparer.Equals(a.AttributeClass, theoryAttributeType))
				: attributes.Any(a => factAttributeType.IsAssignableFrom(a.AttributeClass));
	}

	public static IOperation WalkDownImplicitConversions(this IOperation operation)
	{
		Guard.ArgumentNotNull(operation);

		var current = operation;
		while (current is IConversionOperation conversion && conversion.Conversion.IsImplicit)
			current = conversion.Operand;

		return current;
	}

	sealed class NamedTypeVisitor : SymbolVisitor
	{
		readonly Func<INamedTypeSymbol, bool> selector;

		public NamedTypeVisitor(Func<INamedTypeSymbol, bool> selector) =>
			this.selector = Guard.ArgumentNotNull(selector);

		public INamedTypeSymbol? MatchingType { get; private set; }

		public override void VisitAssembly(IAssemblySymbol symbol) =>
			Guard.ArgumentNotNull(symbol).GlobalNamespace.Accept(this);

		public override void VisitNamespace(INamespaceSymbol symbol)
		{
			Guard.ArgumentNotNull(symbol);

			if (MatchingType is not null)
				return;

			foreach (var member in symbol.GetMembers())
				member.Accept(this);
		}

		public override void VisitNamedType(INamedTypeSymbol symbol)
		{
			Guard.ArgumentNotNull(symbol);

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
