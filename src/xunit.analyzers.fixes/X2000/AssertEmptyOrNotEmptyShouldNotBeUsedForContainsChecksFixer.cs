using System.Composition;
using System.Linq;
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
public class AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer : XunitCodeFixProvider
{
	public const string Key_UseDoesNotContain = "xUnit2029_UseDoesNotContain";
	public const string Key_UseContains = "xUnit2030_UseContains";

	static readonly string[] targetDiagnostics =
	[
		Descriptors.X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck.Id,
		Descriptors.X2030_AssertNotEmptyShouldNotBeUsedForCollectionContainsCheck.Id,
	];

	public AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer() :
		base(targetDiagnostics)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		if (context.Diagnostics.Length != 1)
			return;

		var diagnostic = context.Diagnostics.Single();
		string replaceAssert;
		string equivalenceKey;
		string title;

		if (diagnostic.Id == targetDiagnostics[0])
		{
			replaceAssert = Constants.Asserts.DoesNotContain;
			equivalenceKey = Key_UseDoesNotContain;
			title = "Use DoesNotContain";
		}
		else
		{
			replaceAssert = Constants.Asserts.Contains;
			equivalenceKey = Key_UseContains;
			title = "Use Contains";
		}

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				c => UseCheck(context.Document, invocation, replaceAssert, c),
				equivalenceKey,
				title
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		string replaceAssert,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		var arguments = invocation.ArgumentList.Arguments;
		if (arguments.Count == 1 && arguments[0].Expression is InvocationExpressionSyntax innerInvocationSyntax)
			if (invocation.Expression is MemberAccessExpressionSyntax outerMemberAccess && innerInvocationSyntax.Expression is MemberAccessExpressionSyntax memberAccess)
				if (innerInvocationSyntax.ArgumentList.Arguments[0].Expression is ExpressionSyntax innerArgument)
					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList([Argument(memberAccess.Expression), Argument(innerArgument)])))
							.WithExpression(outerMemberAccess.WithName(IdentifierName(replaceAssert)))
					);

		return editor.GetChangedDocument();
	}
}
