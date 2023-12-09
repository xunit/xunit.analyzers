using System.Composition;
using System.Globalization;
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
public class AssertEqualShouldNotBeUsedForCollectionSizeCheckFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2013_UseAlterateAssert";

	public AssertEqualShouldNotBeUsedForCollectionSizeCheckFixer() :
		base(Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck.Id)
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
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.SizeValue, out var sizeValue))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Use Assert.{0}", replacement),
				ct => UseCollectionSizeAssertionAsync(context.Document, invocation, replacement, ct),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseCollectionSizeAssertionAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			var expression = GetExpressionSyntax(invocation);

			if (expression is not null)
				editor.ReplaceNode(
					invocation,
					invocation
						.WithArgumentList(invocation.ArgumentList.WithArguments(SingletonSeparatedList(Argument(expression))))
						.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
				);
		}

		return editor.GetChangedDocument();
	}

	static ExpressionSyntax? GetExpressionSyntax(InvocationExpressionSyntax invocation)
	{
		if (invocation.ArgumentList.Arguments.Count < 2)
			return null;

		if (invocation.ArgumentList.Arguments[1].Expression is InvocationExpressionSyntax sizeInvocation)
			return (sizeInvocation.Expression as MemberAccessExpressionSyntax)?.Expression;

		var sizeMemberAccess = invocation.ArgumentList.Arguments[1].Expression as MemberAccessExpressionSyntax;
		return sizeMemberAccess?.Expression;
	}
}
