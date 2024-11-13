using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

public static class CodeAnalysisExtensions
{
	public static async Task<Document> AddConstructor(
		this Document document,
		ClassDeclarationSyntax declaration,
		string typeDisplayName,
		string typeName,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(document);
		Guard.ArgumentNotNull(declaration);
		Guard.ArgumentNotNull(typeDisplayName);
		Guard.ArgumentNotNull(typeName);

#pragma warning disable CA1308 // These are display names, not normalizations for comparison

		// TODO: Make this respect the user's preferences on identifier name style
		var fieldName = "_" + typeName.Substring(0, 1).ToLowerInvariant() + typeName.Substring(1);
		var constructorArgName = typeName.Substring(0, 1).ToLowerInvariant() + typeName.Substring(1);
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

#pragma warning restore CA1308

		var fieldDeclaration =
			FieldDeclaration(
				VariableDeclaration(ParseTypeName(typeDisplayName))
				.WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(fieldName))))
			)
			.WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)));

		var constructor =
			ConstructorDeclaration(Identifier(declaration.Identifier.Text))
			.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
			.WithParameterList(
				ParameterList(
					SingletonSeparatedList(
						Parameter(Identifier(constructorArgName))
						.WithType(ParseTypeName(typeDisplayName))
					)
				)
			)
			.WithBody(
				Block(
					SingletonList<StatementSyntax>(
						ExpressionStatement(
							AssignmentExpression(
								SyntaxKind.SimpleAssignmentExpression,
								IdentifierName(fieldName),
								IdentifierName(constructorArgName)
							)
						)
					)
				)
			);

		editor.InsertMembers(declaration, 0, [fieldDeclaration, constructor]);

		return editor.GetChangedDocument();
	}

	public static async Task<Document> ChangeAccessibility(
		this Document document,
		SyntaxNode declaration,
		Accessibility accessibility,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(document);
		Guard.ArgumentNotNull(declaration);

		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		editor.SetAccessibility(declaration, accessibility);
		return editor.GetChangedDocument();
	}

	public static async Task<Solution> ChangeMemberAccessibility(
		this Solution solution,
		ISymbol memberSymbol,
		Accessibility accessibility,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(solution);
		Guard.ArgumentNotNull(memberSymbol);

		var editor = SymbolEditor.Create(solution);

		await editor.EditAllDeclarationsAsync(
			memberSymbol,
			(docEditor, syntaxNode) => docEditor.SetAccessibility(syntaxNode, accessibility),
			cancellationToken
		).ConfigureAwait(false);

		return editor.ChangedSolution;
	}

	public static async Task<Solution> ChangeMemberStaticModifier(
		this Solution solution,
		ISymbol memberSymbol,
		bool isStatic,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(solution);
		Guard.ArgumentNotNull(memberSymbol);

		var editor = SymbolEditor.Create(solution);

		await editor.EditAllDeclarationsAsync(
			memberSymbol,
			(docEditor, syntaxNode) =>
			{
				var newMods = DeclarationModifiers.From(memberSymbol).WithIsStatic(isStatic);
				if (memberSymbol is IPropertySymbol propertySymbol && propertySymbol.IsReadOnly)
				{
					// Looks like there's a bug in Roslyn where SetModifiers applies the 'readonly'
					// keyword to a get-only property, producing illegal syntax.
					newMods = newMods.WithIsReadOnly(false);
				}
				docEditor.SetModifiers(syntaxNode, newMods);
			},
			cancellationToken
		).ConfigureAwait(false);

		return editor.ChangedSolution;
	}

	public static async Task<Solution> ChangeMemberType(
		this Solution solution,
		ISymbol memberSymbol,
		ITypeSymbol type,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(solution);
		Guard.ArgumentNotNull(memberSymbol);
		Guard.ArgumentNotNull(type);

		var editor = SymbolEditor.Create(solution);

		await editor.EditAllDeclarationsAsync(
			memberSymbol,
			(docEditor, syntaxNode) => docEditor.SetType(syntaxNode, docEditor.Generator.TypeExpression(type)),
			cancellationToken
		).ConfigureAwait(false);

		return editor.ChangedSolution;
	}

	public static async Task<Document> RemoveNode(
		this Document document,
		SyntaxNode node,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(document);
		Guard.ArgumentNotNull(node);

		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		editor.RemoveNode(node);

		return editor.GetChangedDocument();
	}

	public static async Task<Document> ExtractNodeFromParent(
		this Document document,
		SyntaxNode node,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(document);
		Guard.ArgumentNotNull(node);

		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var parent = node.Parent;

		if (parent is not null)
		{
			editor.RemoveNode(node);

			var formattedNode =
				node
					.WithLeadingTrivia(ElasticMarker)
					.WithTrailingTrivia(ElasticMarker)
					.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);

			editor.InsertAfter(parent, formattedNode);
		}

		return editor.GetChangedDocument();
	}

	public static async Task<Document> SetBaseClass(
		this Document document,
		ClassDeclarationSyntax declaration,
		string baseType,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(document);
		Guard.ArgumentNotNull(declaration);
		Guard.ArgumentNotNull(baseType);

		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var generator = editor.Generator;
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

		if (semanticModel is not null)
		{
			var baseTypeMetadata = semanticModel.Compilation.GetTypeByMetadataName(baseType);
			if (baseTypeMetadata is not null)
			{
				var baseTypeNode = generator.TypeExpression(baseTypeMetadata);
				var baseTypes = generator.GetBaseAndInterfaceTypes(declaration);

				var updatedDeclaration =
					baseTypes is null || baseTypes.Count == 0 || semanticModel.GetTypeInfo(baseTypes[0], cancellationToken).Type?.TypeKind != TypeKind.Class
						? generator.AddBaseType(declaration, baseTypeNode)
						: generator.ReplaceNode(declaration, baseTypes[0], baseTypeNode);

				editor.ReplaceNode(declaration, updatedDeclaration);
			}
		}

		return editor.GetChangedDocument();
	}
}
