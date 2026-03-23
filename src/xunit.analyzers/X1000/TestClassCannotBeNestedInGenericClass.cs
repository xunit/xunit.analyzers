using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestClassCannotBeNestedInGenericClass : XunitDiagnosticAnalyzer
{
	public TestClassCannotBeNestedInGenericClass() :
		base(Descriptors.X1032_TestClassCannotBeNestedInGenericClass)
	{ }

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
			if (context.Symbol is not INamedTypeSymbol classSymbol)
				return;
			if (classSymbol.ContainingType is null)
				return;
			if (!classSymbol.ContainingType.IsGenericType)
				return;

			var doesClassContainTests = DoesInheritenceTreeContainTests(classSymbol, factAndTheoryAttributeTypes, depth: 3);

			if (!doesClassContainTests)
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1032_TestClassCannotBeNestedInGenericClass,
					classSymbol.Locations.First()
				)
			);
		}, SymbolKind.NamedType);
	}

	static bool DoesInheritenceTreeContainTests(
		INamedTypeSymbol classSymbol,
		ImmutableHashSet<INamedTypeSymbol> factAndTheoryAttributeTypes,
		int depth)
	{
		var doesClassContainTests =
			classSymbol
				.GetMembers()
				.OfType<IMethodSymbol>()
				.Any(m => m.GetAttributes().Any(a => factAndTheoryAttributeTypes.Any(f => f.IsAssignableFrom(a.AttributeClass))));

		if (!doesClassContainTests && classSymbol.BaseType is not null && depth > 0)
			return DoesInheritenceTreeContainTests(classSymbol.BaseType, factAndTheoryAttributeTypes, depth - 1);

		return doesClassContainTests;
	}
}
