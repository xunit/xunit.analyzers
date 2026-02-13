using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class BooleanAssertsShouldNotBeNegatedFixer : XunitCodeFixProvider
{
	public const string Key_UseSuggestedAssert = "xUnit2022_UseSuggestedAssert";

	public BooleanAssertsShouldNotBeNegatedFixer() :
		base(Descriptors.X2022_BooleanAssertionsShouldNotBeNegated.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
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
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => UseSuggestedAssert(context.Document, invocation, replacement, ct),
				Key_UseSuggestedAssert,
				"Use Assert.{0}", replacement
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseSuggestedAssert(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			if (invocation.ArgumentList.Arguments[0].Expression is PrefixUnaryExpressionSyntax prefixUnaryExpression)
			{
				var originalArguments = invocation.ArgumentList.Arguments;
				var newFirstArgument = Argument(prefixUnaryExpression.Operand);

				var newArguments = new List<ArgumentSyntax> { newFirstArgument };
				if (originalArguments.Count > 1)
					newArguments.AddRange(originalArguments.Skip(1));

				editor.ReplaceNode(
					invocation,
					invocation
						.WithArgumentList(ArgumentList(SeparatedList(newArguments)))
						.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
				);
			}
		}

		return editor.GetChangedDocument();
	}
}
