using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class FactMethodMustNotHaveParameters : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1001_FactMethodMustNotHaveParameters);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (xunitContext.Core.FactAttributeType is null)
					return;
				if (context.Symbol is not IMethodSymbol symbol)
					return;
				if (symbol.Parameters.IsEmpty)
					return;

				var attributes = symbol.GetAttributes();
				if (!attributes.IsEmpty && attributes.ContainsAttributeType(xunitContext.Core.FactAttributeType, exactMatch: true))
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1001_FactMethodMustNotHaveParameters,
							symbol.Locations.First(),
							symbol.Name
						)
					);
			}, SymbolKind.Method);
		}
	}
}
