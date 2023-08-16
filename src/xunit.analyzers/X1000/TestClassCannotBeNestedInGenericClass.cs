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
		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null)
				return;
			if (context.Symbol is not INamedTypeSymbol classSymbol)
				return;
			if (classSymbol.ContainingType is null)
				return;
			if (!classSymbol.ContainingType.IsGenericType)
				return;

			var doesClassContainTests = DoesInheritenceTreeContainTests(classSymbol, xunitContext, depth: 3);

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

	private bool DoesInheritenceTreeContainTests(
		INamedTypeSymbol classSymbol,
		XunitContext xunitContext,
		int depth)
	{
		var doesClassContainTests =
			classSymbol
				.GetMembers()
				.OfType<IMethodSymbol>()
				.Any(m => m.GetAttributes().Any(a => xunitContext.Core.FactAttributeType.IsAssignableFrom(a.AttributeClass)));

		if (!doesClassContainTests && classSymbol.BaseType is not null && depth > 0)
			return DoesInheritenceTreeContainTests(classSymbol.BaseType, xunitContext, depth - 1);

		return doesClassContainTests;
	}
}
