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

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		if (!diagnostic.Properties.TryGetValue(Constants.Properties.UseExactMatch, out var useExactMatch))
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		if (useExactMatch == bool.TrueString)
		{
			context.RegisterCodeFix(
				XunitCodeAction.Create(
					ct => UseExactMatchFalse(context.Document, invocation, ct),
					Key_UseAlternateAssert,
					"Use 'exactMatch: false'"
				),
				context.Diagnostics
			);
		}
		else
		{
			var simpleNameSyntax = invocation.GetSimpleName();
			if (simpleNameSyntax is null)
				return;

			var methodName = simpleNameSyntax.Identifier.Text;
			if (!AssertIsTypeShouldNotBeUsedForAbstractType.ReplacementMethods.TryGetValue(methodName, out var replacementName))
				return;

			context.RegisterCodeFix(
				XunitCodeAction.Create(
					ct => UseIsAssignableFrom(context.Document, simpleNameSyntax, replacementName, ct),
					Key_UseAlternateAssert,
					"Use Assert.{0}", replacementName
				),
				context.Diagnostics
			);
		}
	}

	static async Task<Document> UseExactMatchFalse(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		var falseArgument =
			ParseArgumentList("false")
				.Arguments[0]
				.WithNameColon(NameColon("exactMatch"));

		var argumentList = invocation.ArgumentList;
		argumentList =
			argumentList.Arguments.Count == 2
				? argumentList.ReplaceNode(argumentList.Arguments[1], falseArgument)
				: argumentList.AddArguments(falseArgument);

		editor.ReplaceNode(invocation.ArgumentList, argumentList);
		return editor.GetChangedDocument();
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
