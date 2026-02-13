using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class FactMethodMustNotHaveParametersFixer : XunitCodeFixProvider
{
	public const string Key_RemoveParameters = "xUnit1001_RemoveParameters";

	public FactMethodMustNotHaveParametersFixer() :
		base(Descriptors.X1001_FactMethodMustNotHaveParameters.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (methodDeclaration is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Remove parameters",
				ct => RemoveParameters(context.Document, methodDeclaration.ParameterList, ct),
				Key_RemoveParameters
			),
			context.Diagnostics
		);
	}

	static async Task<Document> RemoveParameters(
		Document document,
		ParameterListSyntax parameterListSyntax,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		foreach (var parameter in parameterListSyntax.Parameters)
			editor.RemoveNode(parameter);

		return editor.GetChangedDocument();
	}
}
