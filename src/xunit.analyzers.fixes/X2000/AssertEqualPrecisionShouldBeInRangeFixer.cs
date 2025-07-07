using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertEqualPrecisionShouldBeInRangeFixer : XunitCodeFixProvider
{
	public const string Key_UsePrecision = "xUnit2016_UsePrecision";

	public AssertEqualPrecisionShouldBeInRangeFixer() :
		base(Descriptors.X2016_AssertEqualPrecisionShouldBeInRange.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var precisionArgument = root.FindNode(context.Span).FirstAncestorOrSelf<ArgumentSyntax>();
		if (precisionArgument is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;
		if (!int.TryParse(replacement, out var replacementInt))
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => UseRecommendedPrecision(context.Document, precisionArgument, replacementInt, ct),
				Key_UsePrecision,
				"Use precision {0}", replacementInt
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseRecommendedPrecision(
		Document document,
		ArgumentSyntax precisionArgument,
		int replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.ReplaceNode(
			precisionArgument,
			Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(replacement)))
		);

		return editor.GetChangedDocument();
	}
}
