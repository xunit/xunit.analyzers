using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EnsureFixturesHaveASource : XunitDiagnosticAnalyzer
{
	public EnsureFixturesHaveASource() :
		base(Descriptors.X1041_EnsureFixturesHaveASource)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol namedType)
				return;
			if (namedType.IsAbstract)
				return;
			if (!namedType.IsTestClass(xunitContext))
				return;

			// Only evaluate if there's a single public constructor
			var ctors =
				namedType
					.Constructors
					.Where(c => c is { IsStatic: false, DeclaredAccessibility: Accessibility.Public })
					.ToImmutableArray();
			if (ctors.Length != 1)
				return;

			// Get the collection name from [Collection], if present
			var collectionAttributeType = xunitContext.Core.CollectionAttributeType;
			var collectionDefinitionName =
				namedType
					.GetAttributes()
					.FirstOrDefault(a => a.AttributeClass.IsAssignableFrom(collectionAttributeType))
					?.ConstructorArguments.FirstOrDefault().Value?.ToString();

			// Determine which constructor arguments will come from IClassFixture<>
			var classFixtureInterfaceType = xunitContext.Core.IClassFixtureType?.ConstructUnboundGenericType();
			var classFixtureTypes =
				namedType
					.AllInterfaces
					.Where(i => i.IsGenericType && SymbolEqualityComparer.Default.Equals(classFixtureInterfaceType, i.ConstructUnboundGenericType()))
					.Select(i => i.TypeArguments.First() as INamedTypeSymbol)
					.WhereNotNull()
					.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

			// Determine which constructor arguments will come from ICollectionFixture<> on a fixture definition
			var collectionFixtureTypes = ImmutableHashSet<INamedTypeSymbol>.Empty;
			if (collectionDefinitionName != null)
			{
				var collectionDefinitionAttributeType = xunitContext.Core.CollectionDefinitionAttributeType;

				bool MatchCollectionDefinition(INamedTypeSymbol symbol) =>
					symbol.GetAttributes().Any(a =>
						a.AttributeClass.IsAssignableFrom(collectionDefinitionAttributeType) &&
						!a.ConstructorArguments.IsDefaultOrEmpty &&
						a.ConstructorArguments[0].Value?.ToString() == collectionDefinitionName
					);

				var matchingType = namedType.ContainingAssembly.FindNamedType(MatchCollectionDefinition);
				if (matchingType is not null)
				{
					var collectionFixtureType = xunitContext.Core.ICollectionFixtureType;
					collectionFixtureTypes =
						matchingType
							.AllInterfaces
							.Where(i => i.OriginalDefinition.IsAssignableFrom(collectionFixtureType))
							.Select(i => i.TypeArguments.FirstOrDefault() as INamedTypeSymbol)
							.WhereNotNull()
							.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
				}
			}

			// Determine which constructor arguments will come from IAssemblyFixture
			var assemblyFixtureTypes = ImmutableHashSet<INamedTypeSymbol>.Empty;
			var assemblyFixtureAttributeType = xunitContext.V3Core?.AssemblyFixtureAttributeType;
			if (assemblyFixtureAttributeType is not null)
				assemblyFixtureTypes =
					namedType
						.ContainingAssembly
						.GetAttributes()
						.Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, assemblyFixtureAttributeType))
						.Select(a => a.ConstructorArguments[0].Value as INamedTypeSymbol)
						.WhereNotNull()
						.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

			// Exclude things like ITestOutputHelper and ITestContextAccessor
			var supportedNonFixtureTypes =
				new[] { xunitContext.Core.ITestOutputHelperType, xunitContext.V3Core?.ITestContextAccessorType }
					.WhereNotNull()
					.ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

			foreach (var parameter in ctors[0].Parameters)
				if (!supportedNonFixtureTypes.Contains(parameter.Type, SymbolEqualityComparer.Default)
					&& !classFixtureTypes.Contains(parameter.Type, SymbolEqualityComparer.Default)
					&& !collectionFixtureTypes.Contains(parameter.Type, SymbolEqualityComparer.Default)
					&& !assemblyFixtureTypes.Contains(parameter.Type, SymbolEqualityComparer.Default))
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1041_EnsureFixturesHaveASource,
							parameter.Locations.FirstOrDefault(),
							parameter.Name
						)
					);
				}
		}, SymbolKind.NamedType);
	}
}
