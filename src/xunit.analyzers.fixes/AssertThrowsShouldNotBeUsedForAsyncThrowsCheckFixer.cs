using System.Collections.Immutable;
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

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer : CodeFixProvider
	{
		const string TitleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck.Id,
				Descriptors.X2019_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck.Id
			);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			var diagnostic = context.Diagnostics.First();
			var methodName = diagnostic.Properties[Constants.Properties.MethodName];
			var title = string.Format(TitleTemplate, methodName + "Async");

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => UseAsyncThrowsCheck(context.Document, invocation, method, methodName + "Async", ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		static async Task<Document> UseAsyncThrowsCheck(
			Document document,
			InvocationExpressionSyntax invocation,
			MethodDeclarationSyntax method,
			string replacementMethod,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			var modifiers = GetModifiersWithAsyncKeywordAdded(method);
			var returnType = await GetReturnType(method, invocation, document, editor, cancellationToken).ConfigureAwait(false);
			var asyncThrowsInvocation = GetAsyncThrowsInvocation(invocation, replacementMethod, memberAccess);

			editor.ReplaceNode(
				method,
				method
					.ReplaceNode(invocation, asyncThrowsInvocation)
					.WithModifiers(modifiers)
					.WithReturnType(returnType)
			);

			return editor.GetChangedDocument();
		}

		static SyntaxTokenList GetModifiersWithAsyncKeywordAdded(MethodDeclarationSyntax method) =>
			method.Modifiers.Any(SyntaxKind.AsyncKeyword)
				? method.Modifiers
				: method.Modifiers.Add(Token(SyntaxKind.AsyncKeyword));

		static async Task<TypeSyntax> GetReturnType(
			MethodDeclarationSyntax method,
			InvocationExpressionSyntax invocation,
			Document document,
			DocumentEditor editor,
			CancellationToken cancellationToken)
		{
			// Consider the case where a custom awaiter type is awaited
			if (invocation.Parent.IsKind(SyntaxKind.AwaitExpression))
				return method.ReturnType;

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var methodSymbol = semanticModel.GetSymbolInfo(method.ReturnType, cancellationToken).Symbol as ITypeSymbol;
			var taskType = semanticModel.Compilation.GetTypeByMetadataName(typeof(Task).FullName);

			if (taskType.IsAssignableFrom(methodSymbol))
				return method.ReturnType;

			return (TypeSyntax)editor.Generator.TypeExpression(taskType);
		}

		static ExpressionSyntax GetAsyncThrowsInvocation(
			InvocationExpressionSyntax invocation,
			string replacementMethod,
			MemberAccessExpressionSyntax memberAccess)
		{
			ExpressionSyntax asyncThrowsInvocation =
				invocation
					.WithExpression(memberAccess.WithName(GetName(replacementMethod, memberAccess)))
					.WithArgumentList(GetArguments(invocation));

			if (invocation.Parent.IsKind(SyntaxKind.AwaitExpression))
				return asyncThrowsInvocation;

			return
				AwaitExpression(asyncThrowsInvocation.WithoutLeadingTrivia())
					.WithLeadingTrivia(invocation.GetLeadingTrivia());
		}

		static SimpleNameSyntax GetName(
			string replacementMethod,
			MemberAccessExpressionSyntax memberAccess)
		{
			if (memberAccess.Name is not GenericNameSyntax genericNameSyntax)
				return IdentifierName(replacementMethod);

			return GenericName(IdentifierName(replacementMethod).Identifier, genericNameSyntax.TypeArgumentList);
		}

		static ArgumentListSyntax GetArguments(InvocationExpressionSyntax invocation)
		{
			var arguments = invocation.ArgumentList;
			var argumentSyntax = invocation.ArgumentList.Arguments.Last();

			if (argumentSyntax.Expression is not LambdaExpressionSyntax lambdaExpression)
				return arguments;

			if (lambdaExpression.Body is not AwaitExpressionSyntax awaitExpression)
				return arguments;

			var lambdaExpressionWithoutAsyncKeyword = RemoveAsyncKeywordFromLambdaExpression(lambdaExpression, awaitExpression);

			return
				invocation
					.ArgumentList
					.WithArguments(
						invocation
							.ArgumentList
							.Arguments
							.Replace(argumentSyntax, Argument(lambdaExpressionWithoutAsyncKeyword))
					);
		}

		static ExpressionSyntax RemoveAsyncKeywordFromLambdaExpression(
			LambdaExpressionSyntax lambdaExpression,
			AwaitExpressionSyntax awaitExpression)
		{
			if (lambdaExpression is SimpleLambdaExpressionSyntax simpleLambdaExpression)
				return
					simpleLambdaExpression
						.ReplaceNode(awaitExpression, awaitExpression.Expression)
						.WithAsyncKeyword(default)
						.WithLeadingTrivia(simpleLambdaExpression.AsyncKeyword.LeadingTrivia);

			if (lambdaExpression is ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression)
				return
					parenthesizedLambdaExpression
						.ReplaceNode(awaitExpression, awaitExpression.Expression)
						.WithAsyncKeyword(default)
						.WithLeadingTrivia(parenthesizedLambdaExpression.AsyncKeyword.LeadingTrivia);

			return lambdaExpression;
		}
	}
}
