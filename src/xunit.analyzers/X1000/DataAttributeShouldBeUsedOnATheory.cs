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

		if (xunitContext.Core.DataAttributeType is null)
			return;

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not IMethodSymbol methodSymbol)
				return;

			var attributes = methodSymbol.GetAttributes();
			if (attributes.Length == 0)
				return;

			if (!attributes.ContainsAttributeType(xunitContext.Core.DataAttributeType))
				return;

			// For AOT, only the 4 sealed attribute types are supported
			if (xunitContext.IsAot)
			{
				if (!attributes.ContainsAttributeType(xunitContext.Core.FactAndTheoryAttributeTypes))
					reportX1008();
			}
			// For v3, check for any attribute that implements IFactAttribute
			else if (xunitContext.Core is ICoreContextV3 v3Core && v3Core.IFactAttributeType is { } iFactAttributeType)
			{
				if (!attributes.Any(a => iFactAttributeType.IsAssignableFrom(a.AttributeClass)))
					reportX1008();
			}
			// For v2, check for any attribute that derives from FactAttribute
			else if (xunitContext.Core.FactAttributeType is not null)
			{
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
