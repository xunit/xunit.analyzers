using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class FactMethodMustNotHaveParametersFixer : BatchedCodeFixProvider
{
	public const string RemoveParametersTitle = "Remove Parameters";

	public FactMethodMustNotHaveParametersFixer() :
		base(Descriptors.X1001_FactMethodMustNotHaveParameters.Id)
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
			CodeAction.Create(
				title: RemoveParametersTitle,
				createChangedDocument: ct => RemoveParameters(context.Document, methodDeclaration.ParameterList, ct),
				equivalenceKey: RemoveParametersTitle
			),
			context.Diagnostics
		);
	}

	async Task<Document> RemoveParameters(
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
