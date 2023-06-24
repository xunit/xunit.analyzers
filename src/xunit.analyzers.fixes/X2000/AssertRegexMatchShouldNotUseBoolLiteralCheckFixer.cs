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
public class AssertRegexMatchShouldNotUseBoolLiteralCheckFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2008_UseAlternateAssert";

	public AssertRegexMatchShouldNotUseBoolLiteralCheckFixer() :
		base(Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck.Id)
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
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.IsStatic, out var isStatic))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format("Use Assert.{0}", replacement),
				ct => UseRegexCheckAsync(context.Document, invocation, replacement, isStatic == bool.TrueString, ct),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseRegexCheckAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		bool isStatic,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			if (invocation.ArgumentList.Arguments.Count > 0 && invocation.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax regexIsMatchInvocation)
			{
				if (isStatic)
				{
					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList(regexIsMatchInvocation.ArgumentList.Arguments.Reverse())))
							.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
					);
				}
				else if (regexIsMatchInvocation.ArgumentList.Arguments.Count > 0 && regexIsMatchInvocation.Expression is MemberAccessExpressionSyntax regexMemberAccess)
				{
					var regexMember = regexMemberAccess.Expression;

					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(regexMember), regexIsMatchInvocation.ArgumentList.Arguments[0] })))
							.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
					);
				}
			}

		return editor.GetChangedDocument();
	}
}
