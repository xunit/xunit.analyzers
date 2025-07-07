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
public class AssertSingleShouldUseTwoArgumentCallFixer : XunitCodeFixProvider
{
	public const string Key_UseTwoArguments = "xUnit2031_UseSingleTwoArgumentCall";

	public AssertSingleShouldUseTwoArgumentCallFixer() :
		base(Descriptors.X2031_AssertSingleShouldUseTwoArgumentCall.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				c => UseCheck(context.Document, invocation, c),
				Key_UseTwoArguments,
				"Use two-argument call"
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		var arguments = invocation.ArgumentList.Arguments;
		if (arguments.Count == 1 && arguments[0].Expression is InvocationExpressionSyntax innerInvocationSyntax)
			if (innerInvocationSyntax.Expression is MemberAccessExpressionSyntax memberAccess)
				if (innerInvocationSyntax.ArgumentList.Arguments[0].Expression is ExpressionSyntax innerArgument)
					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList([Argument(memberAccess.Expression), Argument(innerArgument)])))
					);

		return editor.GetChangedDocument();
	}
}
