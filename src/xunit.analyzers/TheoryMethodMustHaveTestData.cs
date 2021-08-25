using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TheoryMethodMustHaveTestData : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		   ImmutableArray.Create(Descriptors.X1003_TheoryMethodMustHaveTestData);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				var symbol = (IMethodSymbol)context.Symbol;
				var attributes = symbol.GetAttributes();
				if (attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType) &&
					(attributes.Length == 1 || !attributes.ContainsAttributeType(xunitContext.Core.DataAttributeType)))
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1003_TheoryMethodMustHaveTestData,
							symbol.Locations.First()
						)
					);
				}
			}, SymbolKind.Method);
		}
	}
}
