using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEqualLiteralValueShouldBeFirst : AssertUsageAnalyzerBase
	{
		public AssertEqualLiteralValueShouldBeFirst()
			: base(Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst, new[] { "Equal", "StrictEqual", "NotEqual", "NotStrictEqual" })
		{ }

		protected override void Analyze(OperationAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var arguments = invocation.ArgumentList.Arguments;
			if (arguments.Count < 2)
				return;

			ArgumentSyntax expectedArg, actualArg;
			if (arguments.All(x => x.NameColon != null))
			{
				expectedArg = arguments.Single(x => x.NameColon.Name.Identifier.ValueText == "expected");
				actualArg = arguments.Single(x => x.NameColon.Name.Identifier.ValueText == "actual");
			}
			else
			{
				expectedArg = arguments[0];
				actualArg = arguments[1];
			}

			if (IsLiteralOrConstant(actualArg.Expression, context.GetSemanticModel(), context.CancellationToken) &&
				!IsLiteralOrConstant(expectedArg.Expression, context.GetSemanticModel(), context.CancellationToken))
			{
				var parentMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
				var parentType = parentMethod.FirstAncestorOrSelf<ClassDeclarationSyntax>();

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst,
						invocation.GetLocation(),
						actualArg.Expression.ToString(),
						SymbolDisplay.ToDisplayString(
							method,
							SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.None).WithParameterOptions(SymbolDisplayParameterOptions.IncludeName)),
						parentMethod.Identifier.ValueText,
						parentType.Identifier.ValueText));
			}
		}

		static bool IsLiteralOrConstant(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			return expression.IsKind(SyntaxKind.DefaultExpression) ||
				   expression.IsKind(SyntaxKind.TypeOfExpression) ||
				   expression.IsKind(SyntaxKind.SizeOfExpression) ||
				   expression is LiteralExpressionSyntax ||
				   expression.IsNameofExpression(semanticModel, cancellationToken) ||
				   expression.IsEnumValueExpression(semanticModel, cancellationToken);
		}
	}
}
