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
		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.FactAttributeType is null)
				return;
			if (context.Symbol is not IMethodSymbol method)
				return;
			if (!method.GetAttributes().Any(a => a.AttributeClass is not null && xunitContext.Core.FactAttributeType.IsAssignableFrom(a.AttributeClass)))
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
					new[] { string.Join(", ", validReturnTypeDisplayNames) }
				)
			);
		}, SymbolKind.Method);
	}

	public List<INamedTypeSymbol> GetValidReturnTypes(
		Compilation compilation,
		XunitContext xunitContext)
	{
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
