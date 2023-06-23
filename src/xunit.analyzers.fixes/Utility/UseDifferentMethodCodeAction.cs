using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

public class UseDifferentMethodCodeAction : CodeAction
{
	readonly Document document;
	readonly InvocationExpressionSyntax invocation;
	readonly string replacementMethod;

	public UseDifferentMethodCodeAction(
		string title,
		Document document,
		InvocationExpressionSyntax invocation,
		string replacementMethod)
	{
		Title = title;

		this.document = document;
		this.invocation = invocation;
		this.replacementMethod = replacementMethod;
	}

	public override string EquivalenceKey => Title;

	public override string Title { get; }

	protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			if (editor.Generator.IdentifierName(replacementMethod) is SimpleNameSyntax replacementNameSyntax)
				editor.ReplaceNode(memberAccess, memberAccess.WithName(replacementNameSyntax));

		return editor.GetChangedDocument();
	}
}
