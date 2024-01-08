using System.Composition;
using System.Globalization;
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
public class UseGenericOverloadFix : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2007_xUnit2015_UseAlternateAssert";

	public UseGenericOverloadFix() :
		base(
			Descriptors.X2007_AssertIsTypeShouldUseGenericOverload.Id,
			Descriptors.X2015_AssertThrowsShouldUseGenericOverload.Id
		)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var syntaxNode = root.FindNode(context.Span);
		var invocation = syntaxNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		if (invocation.ArgumentList.Arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpression)
			return;
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return;

		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		var typeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type);
		if (typeInfo.Type is null)
			return;

		var typeName = SymbolDisplay.ToDisplayString(typeInfo.Type, SymbolDisplayFormat.MinimallyQualifiedFormat);
		var methodName = memberAccess.Name.Identifier.ValueText;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Use Assert.{0}<{1}>", methodName, typeName),
				ct => RemoveTypeofInvocationAndAddGenericTypeAsync(context.Document, invocation, memberAccess, typeOfExpression, ct),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> RemoveTypeofInvocationAndAddGenericTypeAsync(
		Document document,
		InvocationExpressionSyntax invocation,
		MemberAccessExpressionSyntax memberAccess,
		TypeOfExpressionSyntax typeOfExpression,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.ReplaceNode(
			invocation,
			invocation.WithExpression(
				memberAccess.WithName(
					GenericName(
						memberAccess.Name.Identifier,
						TypeArgumentList(SingletonSeparatedList(typeOfExpression.Type))
					)
				)
			)
			.WithArgumentList(
				invocation
					.ArgumentList
					.WithArguments(invocation.ArgumentList.Arguments.RemoveAt(0))
			)
		);

		return editor.GetChangedDocument();
	}
}
