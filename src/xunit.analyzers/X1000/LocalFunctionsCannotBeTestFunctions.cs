using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not LocalFunctionStatementSyntax syntax)
				return;

			var attributeBaseTypes = new List<INamedTypeSymbol>();

			if (xunitContext.Core.FactAttributeType is not null)
				attributeBaseTypes.Add(xunitContext.Core.FactAttributeType);
			if (xunitContext.Core.DataAttributeType is not null)
				attributeBaseTypes.Add(xunitContext.Core.DataAttributeType);

			if (attributeBaseTypes.Count == 0)
				return;

			foreach (var attributeList in syntax.AttributeLists)
				foreach (var attribute in attributeList.Attributes)
				{
					var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
					if (symbol is null)
						continue;

					var attributeType = symbol.ContainingType;
					if (attributeType is null)
						continue;

					foreach (var attributeBaseType in attributeBaseTypes)
						if (attributeBaseType.IsAssignableFrom(attributeType))
							context.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1029_LocalFunctionsCannotBeTestFunctions,
									attribute.GetLocation(),
									$"[{attribute.GetText()}]"
								)
							);
				}
		}, SyntaxKind.LocalFunctionStatement);
	}
}
