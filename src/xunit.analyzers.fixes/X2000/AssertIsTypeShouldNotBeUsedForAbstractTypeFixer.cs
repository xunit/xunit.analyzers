using System.Composition;
using System.Globalization;
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
public class AssertIsTypeShouldNotBeUsedForAbstractTypeFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2018_UseAlternateAssert";

	public AssertIsTypeShouldNotBeUsedForAbstractTypeFixer() :
		base(Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var simpleNameSyntax = invocation.GetSimpleName();
		if (simpleNameSyntax is null)
			return;

		var methodName = simpleNameSyntax.Identifier.Text;
		if (!AssertIsTypeShouldNotBeUsedForAbstractType.ReplacementMethods.TryGetValue(methodName, out var replacementName))
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Use Assert.{0}", replacementName),
				ct => UseIsAssignableFrom(context.Document, simpleNameSyntax, replacementName, ct),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseIsAssignableFrom(
		Document document,
		SimpleNameSyntax simpleName,
		string replacementName,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.ReplaceNode(
			simpleName,
			simpleName.WithIdentifier(Identifier(replacementName))
		);

		return editor.GetChangedDocument();
	}
}
