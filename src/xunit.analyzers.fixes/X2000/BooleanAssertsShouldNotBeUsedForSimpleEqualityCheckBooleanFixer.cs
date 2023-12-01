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
public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer : BatchedCodeFixProvider
{
	public const string Key_UseSuggestedAssert = "xUnit2025_SimplifyBooleanAssert";

	public BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer() :
		base(Descriptors.X2025_BooleanAssertionCanBeSimplified.Id)
	{ }

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

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Simplify the condition and use Assert.{0}", replacement),
				ct => UseSuggestedAssert(context.Document, invocation, replacement, isLeftLiteral == Constants.Asserts.True, ct),
				Key_UseSuggestedAssert
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
				ExpressionSyntax newArgument = isLeftLiteral ? binaryExpressionSyntax.Right : binaryExpressionSyntax.Left;
				editor.ReplaceNode(
					invocation,
					invocation
						.WithArgumentList(ArgumentList(SeparatedList(invocation.ArgumentList.Arguments.Count > 1
							? new[] { Argument(newArgument), invocation.ArgumentList.Arguments[1] }
							: new[] { Argument(newArgument) })))
						.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
				);
			}

		return editor.GetChangedDocument();
	}
}
