using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataAttributeShouldBeUsedOnATheory : XunitDiagnosticAnalyzer
{
	public DataAttributeShouldBeUsedOnATheory() :
		base(Descriptors.X1008_DataAttributeShouldBeUsedOnATheory)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.DataAttributeType is null)
				return;

			if (context.Symbol is not IMethodSymbol methodSymbol)
				return;

			var attributes = methodSymbol.GetAttributes();
			if (attributes.Length == 0)
				return;

			if (!attributes.ContainsAttributeType(xunitContext.Core.DataAttributeType))
				return;

			if (xunitContext.IsAot)
			{
				if (!attributes.ContainsAttributeType(xunitContext.Core.FactAndTheoryAttributeTypes))
					reportX1008();
			}
			else
			{
				// Instead of checking for Theory, we check for any Fact. If it is a Fact which is not a Theory,
				// we will let other rules (i.e. FactMethodShouldNotHaveTestData) handle that case.
				if (!attributes.ContainsAttributeType(xunitContext.Core.FactAttributeType))
					reportX1008();
			}

			void reportX1008()
			{
				var properties = new Dictionary<string, string?>
				{
					[Constants.Properties.DataAttributeTypeName] =
						xunitContext.HasV3References
							? Constants.Types.Xunit.DataAttribute_V3
							: Constants.Types.Xunit.DataAttribute_V2
				}.ToImmutableDictionary();

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1008_DataAttributeShouldBeUsedOnATheory,
						methodSymbol.Locations.First(),
						properties
					)
				);
			}

		}, SymbolKind.Method);
	}
}
