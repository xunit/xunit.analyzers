using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodsFromReferencedAssembliesInAot() :
	XunitV3AotDiagnosticAnalyzer(Descriptors.X1072_TestMethodsFromReferencedAssembliesNotSupported)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var factAndTheoryAttributeTypes = xunitContext.Core.FactAndTheoryAttributeTypes;
		if (factAndTheoryAttributeTypes.Count == 0)
			return;

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class, IsAbstract: false, DeclaredAccessibility: Accessibility.Public } classSymbol)
				return;

			var location = classSymbol.Locations.FirstOrDefault(location => location.IsInSource);
			if (location is null)
				return;

			// A source override with its own test attribute is registered by the generator.
			var sourceTestOverrides = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
			foreach (var sourceMethod in classSymbol.GetInheritedAndOwnMembers().OfType<IMethodSymbol>())
				if (!sourceMethod.DeclaringSyntaxReferences.IsEmpty && !sourceMethod.IsAbstract
						&& sourceMethod.GetAttributes().ContainsAttributeType(factAndTheoryAttributeTypes))
					for (var overriddenMethod = sourceMethod.OverriddenMethod; overriddenMethod is not null; overriddenMethod = overriddenMethod.OverriddenMethod)
						sourceTestOverrides.Add(overriddenMethod.OriginalDefinition);

			for (var baseType = classSymbol.BaseType; baseType is not null; baseType = baseType.BaseType)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (!baseType.DeclaringSyntaxReferences.IsEmpty)
					continue;

				foreach (var method in baseType.GetMembers().OfType<IMethodSymbol>())
				{
					if (method.DeclaredAccessibility != Accessibility.Public
							|| !method.GetAttributes().ContainsAttributeType(factAndTheoryAttributeTypes)
							|| sourceTestOverrides.Contains(method.OriginalDefinition))
						continue;

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1072_TestMethodsFromReferencedAssembliesNotSupported,
							location,
							method.ContainingType.ToDisplayString(),
							method.Name
						)
					);
				}
			}
		}, SymbolKind.NamedType);
	}
}
