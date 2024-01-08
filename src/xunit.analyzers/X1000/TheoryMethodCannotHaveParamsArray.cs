using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TheoryMethodCannotHaveParamsArray : XunitDiagnosticAnalyzer
{
	public TheoryMethodCannotHaveParamsArray() :
		base(Descriptors.X1022_TheoryMethodCannotHaveParameterArray)
	{ }

	protected override bool ShouldAnalyze(XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(xunitContext);

		return base.ShouldAnalyze(xunitContext) && !xunitContext.Core.TheorySupportsParameterArrays;
	}

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		context.RegisterSymbolAction(context =>
		{
			if (xunitContext.Core.TheoryAttributeType is null)
				return;
			if (context.Symbol is not IMethodSymbol method)
				return;

			var parameter = method.Parameters.LastOrDefault();
			if (!(parameter?.IsParams ?? false))
				return;

			var attributes = method.GetAttributes();
			if (attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1022_TheoryMethodCannotHaveParameterArray,
						parameter.DeclaringSyntaxReferences.First().GetSyntax(context.CancellationToken).GetLocation(),
						method.Name,
						method.ContainingType.ToDisplayString(),
						parameter.Name
					)
				);
		}, SymbolKind.Method);
	}
}
