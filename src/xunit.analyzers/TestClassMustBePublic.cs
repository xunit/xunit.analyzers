using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TestClassMustBePublic : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1000_TestClassMustBePublic);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (xunitContext.Core.FactAttributeType is null)
					return;
				if (context.Symbol.DeclaredAccessibility == Accessibility.Public)
					return;
				if (context.Symbol is not INamedTypeSymbol classSymbol)
					return;

				var doesClassContainTests =
					classSymbol
						.GetMembers()
						.OfType<IMethodSymbol>()
						.Any(m => m.GetAttributes().Any(a => xunitContext.Core.FactAttributeType.IsAssignableFrom(a.AttributeClass)));

				if (!doesClassContainTests)
					return;

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1000_TestClassMustBePublic,
						classSymbol.Locations.First(),
						classSymbol.Locations.Skip(1),
						classSymbol.Name
					)
				);

			}, SymbolKind.NamedType);
		}
	}
}
