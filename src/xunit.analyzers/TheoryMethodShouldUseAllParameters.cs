using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class TheoryMethodShouldUseAllParameters : XunitDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Descriptors.X1026_TheoryMethodShouldUseAllParameters);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxNodeContext =>
			{
				var methodSyntax = (MethodDeclarationSyntax)syntaxNodeContext.Node;
				if (methodSyntax.ParameterList.Parameters.Count == 0)
					return;

				var methodSymbol = syntaxNodeContext.SemanticModel.GetDeclaredSymbol(methodSyntax);

				var attributes = methodSymbol.GetAttributes();
				if (!attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
					return;

				AnalyzeTheoryParameters(syntaxNodeContext, methodSyntax, methodSymbol);
			}, SyntaxKind.MethodDeclaration);
		}

		private static void AnalyzeTheoryParameters(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodSyntax, IMethodSymbol methodSymbol)
		{
			var methodBody = (SyntaxNode)methodSyntax.Body ?? methodSyntax.ExpressionBody?.Expression;
			if (methodBody == null)
				return;

			var flowAnalysis = context.SemanticModel.AnalyzeDataFlow(methodBody);
			if (!flowAnalysis.Succeeded)
				return;

			var usedParameters = new HashSet<ISymbol>(flowAnalysis.ReadInside);

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
							parameterSymbol.Name));
				}
			}
		}
	}
}
