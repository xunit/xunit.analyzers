using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodSupportedReturnType : XunitDiagnosticAnalyzer
{
	public TestMethodSupportedReturnType() :
		base(Descriptors.X1028_TestMethodHasInvalidReturnType)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null)
			return;

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not IMethodSymbol method)
				return;
			if (!method.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(xunitContext.Core.FactAttributeType, a.AttributeClass) || SymbolEqualityComparer.Default.Equals(xunitContext.Core.TheoryAttributeType, a.AttributeClass)))
				return;

			var validReturnTypes = GetValidReturnTypes(context.Compilation, xunitContext);
			if (validReturnTypes.Any(t => SymbolEqualityComparer.Default.Equals(method.ReturnType, t)))
				return;

			var validReturnTypeDisplayNames =
				validReturnTypes.Select(
					t => SymbolDisplay.ToDisplayString(
						t,
						SymbolDisplayFormat
							.CSharpShortErrorMessageFormat
							.WithParameterOptions(SymbolDisplayParameterOptions.None)
							.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
					)
				).ToArray();

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1028_TestMethodHasInvalidReturnType,
					method.Locations.FirstOrDefault(),
					[string.Join(", ", validReturnTypeDisplayNames)]
				)
			);
		}, SymbolKind.Method);
	}

	public static List<INamedTypeSymbol> GetValidReturnTypes(
		Compilation compilation,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(compilation);
		Guard.ArgumentNotNull(xunitContext);

		var result = new List<INamedTypeSymbol>();

		void Add(INamedTypeSymbol? symbol)
		{
			if (symbol is not null)
				result!.Add(symbol);
		}

		Add(TypeSymbolFactory.Void(compilation));
		Add(TypeSymbolFactory.Task(compilation));

		if (xunitContext.HasV3References)
			Add(TypeSymbolFactory.ValueTask(compilation));

		return result;
	}
}
