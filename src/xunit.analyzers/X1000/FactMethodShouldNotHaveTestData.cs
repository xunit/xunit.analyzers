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
			if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null || xunitContext.Core.DataAttributeType is null)
				return;

			if (context.Symbol is not IMethodSymbol symbol)
				return;

			var attributes = symbol.GetAttributes();
			if (attributes.Length > 1 &&
				attributes.ContainsAttributeType(xunitContext.Core.FactAttributeType, exactMatch: true) &&
				!attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType) &&
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
