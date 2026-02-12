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
public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer : XunitCodeFixProvider
{
	public const string Key_UseSuggestedAssert = "xUnit2024_UseSuggestedAssert";

	public BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer() :
		base(Descriptors.X2024_BooleanAssertionsShouldNotBeUsedForSimpleEqualityCheck.Id)
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
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.LiteralValue, out var isLeftLiteral))
			return;
		if (replacement is null)
			return;

		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => UseSuggestedAssert(context.Document, invocation, replacement, isLeftLiteral == Constants.Asserts.True, ct),
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
		bool isLeftLiteral,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			if (invocation.ArgumentList.Arguments[0].Expression is BinaryExpressionSyntax binaryExpressionSyntax)
			{
				ArgumentSyntax[] separatedList =
					replacement is Constants.Asserts.Null or Constants.Asserts.NotNull
						? isLeftLiteral
							? [Argument(binaryExpressionSyntax.Right)]
							: [Argument(binaryExpressionSyntax.Left)]
						: isLeftLiteral
							? [Argument(binaryExpressionSyntax.Left), Argument(binaryExpressionSyntax.Right)]
							: [Argument(binaryExpressionSyntax.Right), Argument(binaryExpressionSyntax.Left)];

				editor.ReplaceNode(
					invocation,
					invocation
						.WithArgumentList(ArgumentList(SeparatedList(separatedList)))
						.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
				);
			}

		return editor.GetChangedDocument();
	}
}
