using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TheoryMethodShouldUseAllParameters : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.X1026_TheoryMethodShouldUseAllParameters);

		public override void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext)
		{
			context.RegisterSyntaxNodeAction(context =>
			{
				if (xunitContext.Core.TheoryAttributeType is null)
					return;
				if (context.Node is not MethodDeclarationSyntax methodSyntax)
					return;
				if (methodSyntax.ParameterList.Parameters.Count == 0)
					return;

				var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax);
				var attributes = methodSymbol.GetAttributes();
				if (!attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
					return;

				AnalyzeTheoryParameters(context, methodSyntax, methodSymbol);
			}, SyntaxKind.MethodDeclaration);
		}

		static void AnalyzeTheoryParameters(
			SyntaxNodeAnalysisContext context,
			MethodDeclarationSyntax methodSyntax,
			IMethodSymbol methodSymbol)
		{
			var methodBody = methodSyntax.Body as SyntaxNode ?? methodSyntax.ExpressionBody?.Expression;
			if (methodBody is null)
				return;

			var flowAnalysis = context.SemanticModel.AnalyzeDataFlow(methodBody);
			if (!flowAnalysis.Succeeded)
				return;

			var usedParameters = new HashSet<ISymbol>(flowAnalysis.ReadInside.Concat(flowAnalysis.Captured).Distinct());

			for (var i = 0; i < methodSymbol.Parameters.Length; i++)
			{
				var parameterSymbol = methodSymbol.Parameters[i];

				if (!usedParameters.Contains(parameterSymbol))
				{
					var parameterSyntax = methodSyntax.ParameterList.Parameters[i];

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1026_TheoryMethodShouldUseAllParameters,
							parameterSyntax.Identifier.GetLocation(),
							methodSymbol.Name,
							methodSymbol.ContainingType.Name,
							parameterSymbol.Name
						)
					);
				}
			}
		}
	}
}
