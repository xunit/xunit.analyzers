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

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class TheoryDataShouldNotUseTheoryDataRowFixer() :
	XunitCodeFixProvider(Descriptors.X1052_TheoryDataShouldNotUseITheoryDataRow.Id)
{
	public const string Key_UseIEnumerable = "xUnit1052_UseIEnumerable";

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		foreach (var diagnostic in context.Diagnostics)
		{
			var span = diagnostic.Location.SourceSpan;
			var node = root.FindNode(span);

			if (node is not GenericNameSyntax genericNameNode)
				return;

			if (genericNameNode.TypeArgumentList.Arguments.Count != 1)
				return;

			if (!IsPartOfOnlyTypeDeclaration(genericNameNode))
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Use IEnumerable instead of TheoryData",
					ct => ConvertToIEnumerable(context.Document, genericNameNode, ct),
					Key_UseIEnumerable
				),
				diagnostic
			);
		}
	}

	static bool IsPartOfOnlyTypeDeclaration(GenericNameSyntax genericName)
	{
		var parent = genericName.Parent;

		if (parent is VariableDeclarationSyntax variableDeclaration)
			return variableDeclaration.Variables.All(v => v.Initializer is null);

		if (parent is PropertyDeclarationSyntax propertyDeclaration)
			return propertyDeclaration.Initializer is null;

		return parent is ParameterSyntax or MethodDeclarationSyntax;
	}

	static async Task<Document> ConvertToIEnumerable(
		Document document,
		GenericNameSyntax node,
		CancellationToken ct)
	{
		var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
		var token = SyntaxFactory.IdentifierName("IEnumerable").Identifier;
		var newGenericName = SyntaxFactory.GenericName(token, node.TypeArgumentList);

		editor.ReplaceNode(node, newGenericName);

		return editor.GetChangedDocument();
	}
}
