#if ROSLYN_3_11
#pragma warning disable RS1024 // Incorrectly triggered by Roslyn 3.11
#endif

using System.Collections.Generic;
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
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

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
			string? collectionDefinitionName = null;

			for (var type = namedType; type is not null && collectionDefinitionName is null; type = type.BaseType)
				collectionDefinitionName =
					type
						.GetAttributes()
						.FirstOrDefault(a => a.AttributeClass.IsAssignableFrom(collectionAttributeType))
						?.ConstructorArguments.FirstOrDefault().Value?.ToString();

			// Need to construct a full set of types we know can be resolved. Start with things
			// like ITestOutputHelper and ITestContextAccessor (since they're injected by the framework)
			var validConstructorArgumentTypes = new HashSet<ITypeSymbol?>(SymbolEqualityComparer.Default)
			{
				xunitContext.Core.ITestOutputHelperType,
				xunitContext.V3Core?.ITestContextAccessorType
			};

			// Add types from IClassFixture<> on the class
			var classFixtureType = xunitContext.Core.IClassFixtureType?.ConstructUnboundGenericType();
			validConstructorArgumentTypes.AddRange(
				namedType
					.AllInterfaces
					.Where(i => i.IsGenericType && SymbolEqualityComparer.Default.Equals(classFixtureType, i.ConstructUnboundGenericType()))
					.Select(i => i.TypeArguments.First())
			);

			// Add types from IClassFixture<> and ICollectionFixture<> on the collection definition
			var collectionFixtureTypes = ImmutableHashSet<INamedTypeSymbol>.Empty;
			if (collectionDefinitionName is not null)
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
					var collectionFixtureType = xunitContext.Core.ICollectionFixtureType?.ConstructUnboundGenericType();
					foreach (var @interface in matchingType.AllInterfaces.Where(i => i.IsGenericType))
					{
						var unboundGeneric = @interface.ConstructUnboundGenericType();
						if (SymbolEqualityComparer.Default.Equals(classFixtureType, unboundGeneric)
							|| SymbolEqualityComparer.Default.Equals(collectionFixtureType, unboundGeneric))
						{
							var fixtureTypeSymbol = @interface.TypeArguments.First();
							if (fixtureTypeSymbol is INamedTypeSymbol namedFixtureType)
							{
								if (namedFixtureType.IsGenericType && namedFixtureType.TypeArguments.Any(t => t is ITypeParameterSymbol))
								{
									namedFixtureType = namedFixtureType.ConstructedFrom;
								}
								validConstructorArgumentTypes.Add(namedFixtureType);
							}
						}
					}
				}
			}

			// Add types from AssemblyFixtureAttribute on the assembly
			var assemblyFixtureAttributeType = xunitContext.V3Core?.AssemblyFixtureAttributeType;
			if (assemblyFixtureAttributeType is not null)
				validConstructorArgumentTypes.AddRange(
					namedType
						.ContainingAssembly
						.GetAttributes()
						.Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, assemblyFixtureAttributeType))
						.Select(a => a.ConstructorArguments[0].Value as ITypeSymbol)
				);

			foreach (var parameter in ctors[0].Parameters.Where(p => !p.IsOptional
					&& !validConstructorArgumentTypes.Contains(p.Type)
					&& (p.Type is not INamedTypeSymbol nts || !nts.IsGenericType || !validConstructorArgumentTypes.Contains(nts.ConstructedFrom))))
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1041_EnsureFixturesHaveASource,
						parameter.Locations.FirstOrDefault(),
						parameter.Name
					)
				);
		}, SymbolKind.NamedType);
	}
}
