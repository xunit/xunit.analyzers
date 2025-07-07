using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class MemberDataShouldReferenceValidMember_ParamsForNonMethodFixer : XunitCodeFixProvider
{
	public const string Key_RemoveArgumentsFromMemberData = "xUnit1021_RemoveArgumentsFromMemberData";

	public MemberDataShouldReferenceValidMember_ParamsForNonMethodFixer() :
		base(Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		var diagnosticId = diagnostic.Id;
		var attribute = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();
		if (attribute is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Remove arguments from MemberData",
				ct => RemoveUnneededArguments(context.Document, attribute, context.Span, ct),
				Key_RemoveArgumentsFromMemberData
			),
			context.Diagnostics
		);
	}

	static async Task<Document> RemoveUnneededArguments(
		Document document,
		AttributeSyntax attribute,
		TextSpan span,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (attribute.ArgumentList is not null)
			foreach (var argument in attribute.ArgumentList.Arguments)
				if (argument.Span.OverlapsWith(span))
					editor.RemoveNode(argument);

		return editor.GetChangedDocument();
	}
}
