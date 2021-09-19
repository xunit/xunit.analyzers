using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DataAttributeShouldBeUsedOnATheory : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1008_DataAttributeShouldBeUsedOnATheory);

		public override void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (xunitContext.V2Core is null
						|| xunitContext.V2Core.FactAttributeType is null
						|| xunitContext.V2Core.DataAttributeType is null)
					return;

				if (context.Symbol is not IMethodSymbol methodSymbol)
					return;

				var attributes = methodSymbol.GetAttributes();
				if (attributes.Length == 0)
					return;

				// Instead of checking for Theory, we check for any Fact. If it is a Fact which is not a Theory,
				// we will let other rules (i.e. FactMethodShouldNotHaveTestData) handle that case.
				if (!attributes.ContainsAttributeType(xunitContext.V2Core.FactAttributeType) && attributes.ContainsAttributeType(xunitContext.V2Core.DataAttributeType))
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1008_DataAttributeShouldBeUsedOnATheory,
							methodSymbol.Locations.First()
						)
					);
			}, SymbolKind.Method);
		}
	}
}
