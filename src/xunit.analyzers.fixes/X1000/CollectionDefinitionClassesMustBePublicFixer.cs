using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class CollectionDefinitionClassesMustBePublicFixer : XunitCodeFixProvider
{
	public const string Key_MakeCollectionDefinitionClassPublic = "xUnit1027_MakeCollectionDefinitionClassPublic";

	public CollectionDefinitionClassesMustBePublicFixer() :
		base(Descriptors.X1027_CollectionDefinitionClassMustBePublic.Id)
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
				"Make collection definition class public",
				ct => context.Document.ChangeAccessibility(classDeclaration, Accessibility.Public, ct),
				Key_MakeCollectionDefinitionClassPublic
			),
			context.Diagnostics
		);
	}
}
