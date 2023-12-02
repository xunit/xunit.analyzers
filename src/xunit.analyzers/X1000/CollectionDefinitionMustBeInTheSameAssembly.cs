using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CollectionDefinitionMustBeInTheSameAssembly : XunitDiagnosticAnalyzer
{
	public CollectionDefinitionMustBeInTheSameAssembly() :
		base(Descriptors.X1041_CollectionDefinitionMustBeInTheSameAssembly)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol namedType)
				return;

			var collectionAttributeType = xunitContext.Core.CollectionAttributeType;
			var collectionAttribute = namedType
				.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass.IsAssignableFrom(collectionAttributeType));

			var collectionDefinitionName = collectionAttribute?.ConstructorArguments[0].Value?.ToString();
			if (collectionDefinitionName == null)
				return;

			var ctors = namedType.Constructors
				.Where(c => c is { IsStatic: false, DeclaredAccessibility: Accessibility.Public })
				.ToImmutableArray();

			if (ctors.Count() != 1)
				return;

			var ctor = ctors.First();
			var parameterTypes = ctor.Parameters
				.Select(p => p.Type)
				.ToImmutableHashSet(SymbolEqualityComparer.Default);
			if (parameterTypes.IsEmpty)
				return;

			var visitor = new SymbolAssemblyVisitor(ShortCircuitExpressions(collectionDefinitionName, xunitContext, parameterTypes));

			var currentAssembly = context.Compilation.Assembly;
			visitor.Visit(currentAssembly);
			if (visitor.ShortCircuitTriggered)
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1041_CollectionDefinitionMustBeInTheSameAssembly,
					namedType.Locations.First(),
					collectionDefinitionName,
					currentAssembly.Name
				)
			);
		}, SymbolKind.NamedType);
	}

	private static Func<INamedTypeSymbol, bool> ShortCircuitExpressions(
		string collectionDefinitionName,
		XunitContext xunitContext,
		ImmutableHashSet<ISymbol?> parameterTypes)
	{
		var collectionDefinitionAttributeType = xunitContext.Core.CollectionDefinitionAttributeType;
		var collectionFixtureType = xunitContext.Core.ICollectionFixtureType;

		return symbol =>
		{
			bool CollectionDefinitionWithNonEmptyName(AttributeData a) =>
				a.AttributeClass.IsAssignableFrom(collectionDefinitionAttributeType) &&
				!a.ConstructorArguments.IsDefaultOrEmpty &&
				a.ConstructorArguments[0].Value?.ToString() == collectionDefinitionName;

			bool CoveredByCollectionDefinition(ISymbol? pt) => symbol.AllInterfaces
				.Where(i => i.OriginalDefinition.IsAssignableFrom(collectionFixtureType))
				.Select(i => i.TypeArguments.FirstOrDefault())
				.ToImmutableHashSet(SymbolEqualityComparer.Default)
				.Contains(pt);

			return symbol.GetAttributes().Any(CollectionDefinitionWithNonEmptyName) &&
				   parameterTypes.All(CoveredByCollectionDefinition);
		};
	}
}
