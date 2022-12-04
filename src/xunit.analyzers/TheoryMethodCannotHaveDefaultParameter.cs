using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TheoryMethodCannotHaveDefaultParameter : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter);

		protected override bool ShouldAnalyze(XunitContext xunitContext) =>
			!xunitContext.Core.TheorySupportsDefaultParameterValues;

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSymbolAction(context =>
			{
				if (xunitContext.Core.TheoryAttributeType is null)
					return;
				if (context.Symbol is not IMethodSymbol method)
					return;

				var attributes = method.GetAttributes();
				if (!attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
					return;

				foreach (var parameter in method.Parameters)
				{
					context.CancellationToken.ThrowIfCancellationRequested();

					if (parameter.HasExplicitDefaultValue)
					{
						var syntaxNode =
							parameter
								.DeclaringSyntaxReferences
								.First()
								.GetSyntax(context.CancellationToken)
								.FirstAncestorOrSelf<ParameterSyntax>();

						context.ReportDiagnostic(
							Diagnostic.Create(
								Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter,
								syntaxNode.Default.GetLocation(),
								method.Name,
								method.ContainingType.ToDisplayString(),
								parameter.Name
							)
						);
					}
				}
			}, SymbolKind.Method);
		}
	}
}
