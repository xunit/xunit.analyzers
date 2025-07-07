using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class MemberDataShouldReferenceValidMember_NameOfFixer : XunitCodeFixProvider
{
	public const string Key_UseNameof = "xUnit1014_UseNameof";

	public MemberDataShouldReferenceValidMember_NameOfFixer() :
		base(Descriptors.X1014_MemberDataShouldUseNameOfOperator.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		var diagnosticId = diagnostic.Id;
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var attributeArgument = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeArgumentSyntax>();
		if (attributeArgument is null)
			return;

		if (attributeArgument.Expression is LiteralExpressionSyntax memberNameExpression)
		{
			var memberType = default(INamedTypeSymbol);
			if (diagnostic.Properties.TryGetValue(Constants.AttributeProperties.DeclaringType, out var memberTypeName) && memberTypeName is not null)
			{
				var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
				if (semanticModel is not null)
					memberType = semanticModel.Compilation.GetTypeByMetadataName(memberTypeName);
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					"Use nameof",
					ct => UseNameOf(context.Document, memberNameExpression, memberType, ct),
					Key_UseNameof
				),
				context.Diagnostics
			);
		}
	}

	static async Task<Document> UseNameOf(
		Document document,
		LiteralExpressionSyntax memberNameExpression,
		INamedTypeSymbol? memberType,
		CancellationToken cancellationToken)
	{
		var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		documentEditor.ReplaceNode(
			memberNameExpression,
			(node, generator) =>
			{
				var nameofParam = generator.IdentifierName(memberNameExpression.Token.ValueText);

				if (memberType is not null)
					nameofParam = generator.MemberAccessExpression(generator.TypeExpression(memberType), nameofParam);

				return generator.InvocationExpression(generator.IdentifierName("nameof"), nameofParam);
			}
		);

		return documentEditor.GetChangedDocument();
	}
}
