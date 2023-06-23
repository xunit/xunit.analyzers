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
public class AssertEqualShouldNotBeUsedForNullCheckFixer : BatchedCodeFixProvider
{
	const string titleTemplate = "Use Assert.{0}";

	public AssertEqualShouldNotBeUsedForNullCheckFixer() :
		base(Descriptors.X2003_AssertEqualShouldNotUsedForNullCheck.Id)
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

		if (invocation.Expression is MemberAccessExpressionSyntax)
		{
			var title = string.Format(titleTemplate, replacement);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => UseNullCheckAsync(context.Document, invocation, replacement, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}
	}

	static async Task<Document> UseNullCheckAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.ArgumentList.Arguments.Count > 1 && invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			editor.ReplaceNode(
				invocation,
				invocation
					.WithArgumentList(invocation.ArgumentList.WithArguments(SingletonSeparatedList(invocation.ArgumentList.Arguments[1])))
					.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
			);

		return editor.GetChangedDocument();
	}
}
