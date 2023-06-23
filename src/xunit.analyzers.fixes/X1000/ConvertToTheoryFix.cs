using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ConvertToTheoryFix : BatchedCodeFixProvider
{
	public const string ConvertToTheoryTitle = "Convert to Theory";

	public ConvertToTheoryFix() :
		base(
			Descriptors.X1001_FactMethodMustNotHaveParameters.Id,
			Descriptors.X1005_FactMethodShouldNotHaveTestData.Id
		)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (methodDeclaration is null)
			return;

		context.RegisterCodeFix(
			new ConvertAttributeCodeAction(
				ConvertToTheoryTitle,
				context.Document,
				methodDeclaration.AttributeLists,
				fromTypeName: Constants.Types.XunitFactAttribute,
				toTypeName: Constants.Types.XunitTheoryAttribute
			),
			context.Diagnostics
		);
	}
}
