using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssignableFromAssertionIsConfusinglyNamedFixer : BatchedCodeFixProvider
{
	public const string Key_UseIsType = "xUnit2032_UseIsType";

	public AssignableFromAssertionIsConfusinglyNamedFixer() :
		base(Descriptors.X2032_AssignableFromAssertionIsConfusinglyNamed.Id)
	{ }

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var simpleNameSyntax = invocation.GetSimpleName();
		if (simpleNameSyntax is null)
			return;

		var methodName = simpleNameSyntax.Identifier.Text;
		if (!AssignableFromAssertionIsConfusinglyNamed.ReplacementMethods.TryGetValue(methodName, out var replacementName))
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => UseIsType(context.Document, invocation, simpleNameSyntax, replacementName, ct),
				Key_UseIsType,
				"Use Assert.{0}", replacementName
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseIsType(
		Document document,
		InvocationExpressionSyntax invocation,
		SimpleNameSyntax simpleName,
		string replacementName,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.ReplaceNode(
			invocation,
			invocation
				.ReplaceNode(
					simpleName,
					simpleName.WithIdentifier(Identifier(replacementName))
				)
				.WithArgumentList(
					invocation
						.ArgumentList
						.AddArguments(
							ParseArgumentList("false")
								.Arguments[0]
								.WithNameColon(NameColon("exactMatch"))
						)
				)
		);

		return editor.GetChangedDocument();
	}
}
