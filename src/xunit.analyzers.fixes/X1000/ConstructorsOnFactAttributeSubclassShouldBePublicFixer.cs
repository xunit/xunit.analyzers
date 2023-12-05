using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ConstructorsOnFactAttributeSubclassShouldBePublicFixer : BatchedCodeFixProvider
{
	public const string Key_MakeConstructorPublic = "xUnit1043_MakeConstructorPublic";

	public ConstructorsOnFactAttributeSubclassShouldBePublicFixer() :
		base(Descriptors.X1043_ConstructorsOnFactAttributeSubclassShouldBePublic.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var constructorDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
		if (constructorDeclaration is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Make constructor public",
				ct => context.Document.ChangeAccessibility(constructorDeclaration, Accessibility.Public, ct),
				Key_MakeConstructorPublic
			),
			context.Diagnostics
		);
	}
}
