using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnusedCollectionDefinitions : XunitDiagnosticAnalyzer
{
	public UnusedCollectionDefinitions() :
		base(Descriptors.X1070_CollectionDefinitionIsNeverUsed)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var collectionAttributeType = xunitContext.Core.CollectionAttributeType;
		var collectionDefinitionAttributeType = xunitContext.Core.CollectionDefinitionAttributeType;
		if (collectionAttributeType is null || collectionDefinitionAttributeType is null)
			return;

		var collectionBehaviorAttributeType = xunitContext.Core.CollectionBehaviorAttributeType;
		if (collectionBehaviorAttributeType is not null
				&& context.Compilation.Assembly.GetAttributes().Any(a =>
					collectionBehaviorAttributeType.IsAssignableFrom(a.AttributeClass)
					&& a.ConstructorArguments.Length != 0))
			return;

		var collectionAttributeOfTType = xunitContext.V3Core?.CollectionAttributeOfTType?.ConstructUnboundGenericType();

		var definitions = new ConcurrentBag<(INamedTypeSymbol Type, string? Name)>();
		var referencedNames = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);
		var referencedTypes = new ConcurrentDictionary<INamedTypeSymbol, bool>(SymbolEqualityComparer.Default);

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol namedType)
				return;

			foreach (var attribute in namedType.GetAttributes())
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (collectionDefinitionAttributeType.IsAssignableFrom(attribute.AttributeClass))
					definitions.Add((namedType, attribute.ConstructorArguments.FirstOrDefault().Value as string));
				else if (collectionAttributeType.IsAssignableFrom(attribute.AttributeClass))
				{
					var argument = attribute.ConstructorArguments.FirstOrDefault().Value;
					if (argument is string name)
						referencedNames.TryAdd(name, true);
					else if (argument is INamedTypeSymbol type)
						referencedTypes.TryAdd(type, true);
				}
				else if (collectionAttributeOfTType is not null
					&& attribute.AttributeClass is { IsGenericType: true } attributeClass
					&& SymbolEqualityComparer.Default.Equals(collectionAttributeOfTType, attributeClass.ConstructUnboundGenericType())
					&& attributeClass.TypeArguments.FirstOrDefault() is INamedTypeSymbol typeArgument)
					referencedTypes.TryAdd(typeArgument, true);
			}
		}, SymbolKind.NamedType);

		context.RegisterCompilationEndAction(context =>
		{
			foreach (var (type, name) in definitions)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (name is not null && referencedNames.ContainsKey(name))
					continue;
				if (referencedTypes.ContainsKey(type))
					continue;

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1070_CollectionDefinitionIsNeverUsed,
						type.Locations.First(),
						type.Locations.Skip(1),
						name ?? type.Name
					)
				);
			}
		});
	}
}
