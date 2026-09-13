using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LocalFunctionsCannotBeTestFunctions : XunitDiagnosticAnalyzer
{
	public LocalFunctionsCannotBeTestFunctions() :
		base(Descriptors.X1029_LocalFunctionsCannotBeTestFunctions)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var attributeBaseTypes =
			xunitContext.Core.FactAndTheoryAttributeTypes
				.Concat(xunitContext.Core.DataAttributeTypes)
				.ToArray();

		if (attributeBaseTypes.Length == 0)
			return;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not ILocalFunctionOperation localFunction)
				return;

			foreach (var attribute in localFunction.Symbol.GetAttributes())
			{
				if (attribute.AttributeClass is not INamedTypeSymbol attributeType)
					continue;

				if (!attributeBaseTypes.Any(attributeBaseType => attributeBaseType.IsAssignableFrom(attributeType)))
					continue;

				var attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken);
				if (attributeSyntax is null)
					continue;

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1029_LocalFunctionsCannotBeTestFunctions,
						attributeSyntax.GetLocation(),
						$"[{attributeSyntax.GetText()}]"
					)
				);
			}
		}, OperationKind.LocalFunction);
	}
}
