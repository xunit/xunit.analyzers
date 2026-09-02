using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FactMethodShouldNotHaveTestData : XunitDiagnosticAnalyzer
{
	public FactMethodShouldNotHaveTestData() :
		base(Descriptors.X1005_FactMethodShouldNotHaveTestData)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			var factAttributeTypes = xunitContext.Core.FactAttributeTypes;
			var theoryAttributeTypes = xunitContext.Core.TheoryAttributeTypes;
			if ((factAttributeTypes.Count == 0 && theoryAttributeTypes.Count == 0) || xunitContext.Core.DataAttributeType is null)
				return;

			if (context.Symbol is not IMethodSymbol symbol)
				return;

			var attributes = symbol.GetAttributes();
			if (attributes.Length > 1 &&
				attributes.ContainsAttributeType(factAttributeTypes, exactMatch: true) &&
				!attributes.ContainsAttributeType(theoryAttributeTypes) &&
				attributes.ContainsAttributeType(xunitContext.Core.DataAttributeType))
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
						Descriptors.X1005_FactMethodShouldNotHaveTestData,
						symbol.Locations.First(),
						properties
					)
				);
			}
		}, SymbolKind.Method);
	}
}
