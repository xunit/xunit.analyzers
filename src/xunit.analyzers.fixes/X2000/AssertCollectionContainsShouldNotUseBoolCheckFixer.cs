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
public class AssertCollectionContainsShouldNotUseBoolCheckFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2017_UseAlternateAssert";

	public AssertCollectionContainsShouldNotUseBoolCheckFixer() :
		base(Descriptors.X2017_AssertCollectionContainsShouldNotUseBoolCheck.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.MethodName, out var methodName))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format("Use Assert.{0}", replacement),
				ct => UseContainsCheck(context.Document, invocation, replacement, ct),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseContainsCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			if (invocation.ArgumentList.Arguments.Count > 0 && invocation.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax invocationExpressionSyntax)
				if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax anyMethodInvocation)
				{
					var anyTarget = anyMethodInvocation.Expression;

					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList(invocationExpressionSyntax.ArgumentList.Arguments.Insert(1, Argument(anyTarget)))))
							.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
					);
				}

		return editor.GetChangedDocument();
	}
}
