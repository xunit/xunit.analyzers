#pragma warning disable RS1024 // Incorrectly triggered by Roslyn 3.11

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodMustNotHaveMultipleFactAttributes : XunitDiagnosticAnalyzer
{
	public TestMethodMustNotHaveMultipleFactAttributes() :
		base(Descriptors.X1002_TestMethodMustNotHaveMultipleFactAttributes)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null)
				return;
			if (context.Symbol is not IMethodSymbol symbol)
				return;

			var attributeTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

			var count = 0;

			foreach (var attribute in symbol.GetAttributes())
			{
				var attributeType = attribute.AttributeClass;
				if (attributeType is not null && xunitContext.Core.FactAttributeType.IsAssignableFrom(attributeType))
				{
					attributeTypes.Add(attributeType);
					count++;
				}
			}

			if (count > 1)
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1002_TestMethodMustNotHaveMultipleFactAttributes,
						symbol.Locations.First(),
						properties: attributeTypes.ToImmutableDictionary(t => t.ToDisplayString(), t => (string?)string.Empty)
					)
				);
		}, SymbolKind.Method);
	}
}
