using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class FactMethodShouldNotHaveTestDataFixer : BatchedCodeFixProvider
{
	public const string Key_RemoveDataAttributes = "xUnit1005_RemoveDataAttributes";

	public FactMethodShouldNotHaveTestDataFixer() :
		base(Descriptors.X1005_FactMethodShouldNotHaveTestData.Id)
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
			new RemoveAttributesOfTypeCodeAction(
				"Remove data attributes",
				Key_RemoveDataAttributes,
				context.Document,
				methodDeclaration.AttributeLists,
				Constants.Types.Xunit.Sdk.DataAttribute
			),
			context.Diagnostics
		);
	}
}
