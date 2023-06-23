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
public class AssertSubstringCheckShouldNotUseBoolCheckFixer : BatchedCodeFixProvider
{
	const string titleTemplate = "Use Assert.{0}";

	public AssertSubstringCheckShouldNotUseBoolCheckFixer() :
		base(Descriptors.X2009_AssertSubstringCheckShouldNotUseBoolCheck.Id)
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
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.AssertMethodName, out var assertMethodName))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.SubstringMethodName, out var substringMethodName))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		var title = string.Format(titleTemplate, replacement);

		context.RegisterCodeFix(
			CodeAction.Create(
				title,
				createChangedDocument: ct => UseSubstringCheckAsync(context.Document, invocation, replacement, ct),
				equivalenceKey: title
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseSubstringCheckAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			if (invocation.ArgumentList.Arguments.Count > 0 && invocation.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax substringInvocation)
				if (substringInvocation.Expression is MemberAccessExpressionSyntax substringMethodInvocation)
				{
					var substringTarget = substringMethodInvocation.Expression;

					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList(substringInvocation.ArgumentList.Arguments.Insert(1, Argument(substringTarget)))))
							.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
					);
				}

		return editor.GetChangedDocument();
	}
}
