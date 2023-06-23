using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class FactMethodShouldNotHaveTestDataFixer : BatchedCodeFixProvider
{
	const string removeDataAttributesTitle = "Remove Data Attributes";

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
			new RemoveAttributesOfTypeCodeAction(removeDataAttributesTitle, context.Document, methodDeclaration.AttributeLists, Constants.Types.XunitSdkDataAttribute),
			context.Diagnostics
		);
	}
}
