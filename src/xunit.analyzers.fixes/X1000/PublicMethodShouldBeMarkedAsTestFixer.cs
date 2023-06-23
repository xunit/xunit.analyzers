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
	public const string ConvertToFactTitle = "Convert to Fact";
	public const string ConvertToTheoryTitle = "Convert to Theory";
	public const string MakeInternalTitle = "Make Internal";

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

		var looksLikeTheory = methodDeclaration.ParameterList.Parameters.Any();
		var convertTitle = looksLikeTheory ? ConvertToTheoryTitle : ConvertToFactTitle;
		var convertType = looksLikeTheory ? Constants.Types.XunitTheoryAttribute : Constants.Types.XunitFactAttribute;

		context.RegisterCodeFix(
			CodeAction.Create(
				title: convertTitle,
				createChangedDocument: ct => AddAttribute(context.Document, methodDeclaration, convertType, ct),
				equivalenceKey: convertTitle
			),
			context.Diagnostics
		);

		context.RegisterCodeFix(
			CodeAction.Create(
				title: MakeInternalTitle,
				createChangedDocument: ct => context.Document.ChangeAccessibility(methodDeclaration, Accessibility.Internal, ct),
				equivalenceKey: MakeInternalTitle
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
