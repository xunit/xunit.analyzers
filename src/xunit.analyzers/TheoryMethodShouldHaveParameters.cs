using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TheoryMethodShouldHaveParameters : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		   ImmutableArray.Create(Descriptors.X1006_TheoryMethodShouldHaveParameters);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (xunitContext.Core.TheoryAttributeType is null)
					return;
				if (context.Symbol is not IMethodSymbol symbol)
					return;
				if (symbol.Parameters.Length > 0)
					return;

				var attributes = symbol.GetAttributes();
				if (attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1006_TheoryMethodShouldHaveParameters,
							symbol.Locations.First()
						)
					);
			}, SymbolKind.Method);
		}
	}
}
