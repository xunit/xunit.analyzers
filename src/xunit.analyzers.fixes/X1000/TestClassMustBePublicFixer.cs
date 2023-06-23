using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class TestClassMustBePublicFixer : BatchedCodeFixProvider
{
	const string title = "Make Public";

	public TestClassMustBePublicFixer() :
		base(Descriptors.X1000_TestClassMustBePublic.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classDeclaration is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				title: title,
				createChangedDocument: ct => context.Document.ChangeAccessibility(classDeclaration, Accessibility.Public, ct),
				equivalenceKey: title
			),
			context.Diagnostics
		);
	}
}
