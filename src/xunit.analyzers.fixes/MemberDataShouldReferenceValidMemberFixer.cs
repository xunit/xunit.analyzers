using System;
using System.Collections.Immutable;
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

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class MemberDataShouldReferenceValidMemberFixer : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
			= ImmutableArray.Create(Descriptors.X1014_MemberDataShouldUseNameOfOperator.Id, Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters.Id);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnosticId = context.Diagnostics.Single().Id;
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (diagnosticId == Descriptors.X1014_MemberDataShouldUseNameOfOperator.Id)
			{
				var attributeArgument = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeArgumentSyntax>();
				var memberNameExpression = (LiteralExpressionSyntax)attributeArgument.Expression;
				INamedTypeSymbol memberType = null;
				if (context.Diagnostics.First().Properties.TryGetValue("DeclaringType", out string memberTypeName))
				{
					var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
					memberType = semanticModel.Compilation.GetTypeByMetadataName(memberTypeName);
				}

				context.RegisterCodeFix(
					CodeAction.Create(
						"Use nameof",
						createChangedDocument: ct => UseNameof(context.Document, memberNameExpression, memberType, ct),
						equivalenceKey: "Use nameof"),
					context.Diagnostics);
			}
			else if (diagnosticId == Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters.Id)
			{
				var attribute = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeSyntax>();

				context.RegisterCodeFix(
					CodeAction.Create(
						"Remove Arguments",
						createChangedDocument: ct => RemoveUnneededArgumentsAsync(context.Document, attribute, context.Span, ct),
						equivalenceKey: "Remove MemberData Arguments"),
					context.Diagnostics);
			}
		}

		private async Task<Document> UseNameof(Document document, LiteralExpressionSyntax memberNameExpression, INamedTypeSymbol memberType, CancellationToken cancellationToken)
		{
			var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			documentEditor.ReplaceNode(memberNameExpression, (node, generator) =>
			{
				var nameofParam = generator.IdentifierName(memberNameExpression.Token.ValueText);
				if (memberType != null)
					nameofParam = generator.MemberAccessExpression(generator.TypeExpression(memberType), nameofParam);
				return generator.InvocationExpression(generator.IdentifierName("nameof"), nameofParam);
			});

			return documentEditor.GetChangedDocument();
		}

		async Task<Document> RemoveUnneededArgumentsAsync(Document document, AttributeSyntax attribute, TextSpan span, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			foreach (var argument in attribute.ArgumentList.Arguments)
				if (argument.Span.OverlapsWith(span))
					editor.RemoveNode(argument);

			return editor.GetChangedDocument();
		}
	}
}
