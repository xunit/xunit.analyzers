using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ConvertToFactFix : BatchedCodeFixProvider
{
	const string title = "Convert to Fact";

	public ConvertToFactFix() :
		base(
			Descriptors.X1003_TheoryMethodMustHaveTestData.Id,
			Descriptors.X1006_TheoryMethodShouldHaveParameters.Id
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
				title,
				context.Document,
				methodDeclaration.AttributeLists,
				fromTypeName: Constants.Types.XunitTheoryAttribute,
				toTypeName: Constants.Types.XunitFactAttribute
			),
			context.Diagnostics
		);
	}
}
