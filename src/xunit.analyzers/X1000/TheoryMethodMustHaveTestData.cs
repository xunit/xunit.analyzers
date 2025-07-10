using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TheoryMethodMustHaveTestData : XunitDiagnosticAnalyzer
{
	public TheoryMethodMustHaveTestData() :
		base(Descriptors.X1003_TheoryMethodMustHaveTestData)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.TheoryAttributeType is null || xunitContext.Core.DataAttributeType is null)
				return;

			if (context.Symbol is not IMethodSymbol symbol)
				return;

			var attributes = symbol.GetAttributes();
			if (!attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
				return;

			var hasData = attributes.ContainsAttributeType(xunitContext.Core.DataAttributeType);
			if (!hasData && xunitContext.V3Core?.IDataAttributeType is not null)
				hasData = attributes.ContainsAttributeType(xunitContext.V3Core.IDataAttributeType);

			if (!hasData)
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1003_TheoryMethodMustHaveTestData,
						symbol.Locations.First()
					)
				);
		}, SymbolKind.Method);
	}
}
