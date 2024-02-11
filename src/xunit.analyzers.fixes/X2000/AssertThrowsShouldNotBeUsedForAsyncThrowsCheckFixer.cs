using System.Composition;
using System.Globalization;
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
public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2014_UseAlternateAssert";

	public AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer() :
		base(Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck.Id)
	{ }

	static ExpressionSyntax GetAsyncThrowsInvocation(
		InvocationExpressionSyntax invocation,
		string memberName,
		MemberAccessExpressionSyntax memberAccess)
	{
		var asyncThrowsInvocation =
			invocation
				.WithExpression(memberAccess.WithName(GetName(memberName, memberAccess)))
				.WithArgumentList(invocation.ArgumentList);

		if (invocation.Parent.IsKind(SyntaxKind.AwaitExpression))
			return asyncThrowsInvocation;

		return
			AwaitExpression(asyncThrowsInvocation.WithoutLeadingTrivia())
				.WithLeadingTrivia(invocation.GetLeadingTrivia());
	}

	static SimpleNameSyntax GetName(
		string memberName,
		MemberAccessExpressionSyntax memberAccess)
	{
		if (memberAccess.Name is not GenericNameSyntax genericNameSyntax)
			return IdentifierName(memberName);

		return GenericName(IdentifierName(memberName).Identifier, genericNameSyntax.TypeArgumentList);
	}

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (method is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.MethodName, out var methodName))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Use Assert.{0}", replacement),
				ct => UseAsyncThrowsCheck(context.Document, invocation, method, replacement, ct),
				Key_UseAlternateAssert
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseAsyncThrowsCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		MethodDeclarationSyntax method,
		string replacement,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
		{
			var modifiers = AsyncHelper.GetModifiersWithAsyncKeywordAdded(method.Modifiers);
			var returnType = await AsyncHelper.GetAsyncReturnType(method.ReturnType, editor, cancellationToken).ConfigureAwait(false);
			var asyncThrowsInvocation = GetAsyncThrowsInvocation(invocation, replacement, memberAccess);

			if (returnType is not null)
				editor.ReplaceNode(
					method,
					method
						.ReplaceNode(invocation, asyncThrowsInvocation)
						.WithModifiers(modifiers)
						.WithReturnType(returnType)
				);
		}

		return editor.GetChangedDocument();
	}
}
