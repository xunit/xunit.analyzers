using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.FixProviders
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class UseGenericOverloadFix : CodeFixProvider
	{
		const string TitleTemplate = "Use Assert.{0}<{1}>";
		const string EquivalenceKeyTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.X2007_AssertIsTypeShouldUseGenericOverload.Id,
				Descriptors.X2015_AssertThrowsShouldUseGenericOverload.Id
			);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var syntaxNode = root.FindNode(context.Span);
			var invocation = syntaxNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();

			if (invocation.ArgumentList.Arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpression)
				return;
			if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
				return;

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			var typeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type);
			var typeName = SymbolDisplay.ToDisplayString(typeInfo.Type, SymbolDisplayFormat.MinimallyQualifiedFormat);
			var methodName = memberAccess.Name.Identifier.ValueText;
			var title = string.Format(TitleTemplate, methodName, typeName);
			var equivalenceKey = string.Format(EquivalenceKeyTemplate, methodName);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => RemoveTypeofInvocationAndAddGenericTypeAsync(context.Document, invocation, memberAccess, typeOfExpression, ct),
					equivalenceKey: equivalenceKey
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
}
