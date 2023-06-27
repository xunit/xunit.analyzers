using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class PublicMethodShouldBeMarkedAsTestFixer : BatchedCodeFixProvider
{
	public const string Key_ConvertToFact = "xUnit1013_ConvertToFact";
	public const string Key_ConvertToTheory = "xUnit1013_ConvertToTheory";
	public const string Key_MakeMethodInternal = "xUnit1013_MakeMethodInternal";

	public PublicMethodShouldBeMarkedAsTestFixer() :
		base(Descriptors.X1013_PublicMethodShouldBeMarkedAsTest.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (methodDeclaration is null)
			return;

		// Fix #1: Offer to convert it to a theory if it has parameters...
		if (methodDeclaration.ParameterList.Parameters.Any())
			context.RegisterCodeFix(
				CodeAction.Create(
					"Add [Theory]",
					ct => AddAttribute(context.Document, methodDeclaration, Constants.Types.Xunit.TheoryAttribute, ct),
					Key_ConvertToTheory
				),
				context.Diagnostics
			);
		// ...otherwise, offer to convert it to a fact
		else
			context.RegisterCodeFix(
				CodeAction.Create(
					"Add [Fact]",
					ct => AddAttribute(context.Document, methodDeclaration, Constants.Types.Xunit.FactAttribute, ct),
					Key_ConvertToFact
				),
				context.Diagnostics
			);

		// Fix #2: Offer to mark the method as internal
		context.RegisterCodeFix(
			CodeAction.Create(
				"Make method internal",
				ct => context.Document.ChangeAccessibility(methodDeclaration, Accessibility.Internal, ct),
				Key_MakeMethodInternal
			),
			context.Diagnostics
		);
	}

	async Task<Document> AddAttribute(
		Document document,
		MethodDeclarationSyntax methodDeclaration,
		string type,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.AddAttribute(methodDeclaration, editor.Generator.Attribute(type));

		return editor.GetChangedDocument();
	}
}
