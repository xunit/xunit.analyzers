using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.CodeActions
{
	public static class Actions
	{
		public static async Task<Document> ChangeAccessibility(Document document, SyntaxNode declaration, Accessibility accessibility, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			editor.SetAccessibility(declaration, accessibility);
			return editor.GetChangedDocument();
		}

		public static async Task<Document> AddConstructor(
			Document document,
			ClassDeclarationSyntax declaration,
			string typeDisplayName,
			string typeName,
			CancellationToken cancellationToken)
		{
			// todo make this respect the user's preferences on identiifer name style
			var fieldName = "_" + typeName.Substring(0, 1).ToLower() + typeName.Substring(1, typeName.Length - 1);
			var constructorArgName = typeName.Substring(0, 1).ToLower() + typeName.Substring(1, typeName.Length - 1);
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			var fieldDeclaration = FieldDeclaration(
						VariableDeclaration(
							ParseTypeName(typeDisplayName))
						.WithVariables(
							SingletonSeparatedList(
								VariableDeclarator(
									Identifier(fieldName)))))
					.WithModifiers(
						TokenList(
								Token(SyntaxKind.PrivateKeyword),
								Token(SyntaxKind.ReadOnlyKeyword)));

			var constructor = ConstructorDeclaration(
						Identifier(declaration.Identifier.Text))
					.WithParameterList(
						ParameterList(
							SingletonSeparatedList(
								Parameter(
									Identifier(constructorArgName))
								.WithType(
									ParseTypeName(typeDisplayName)))))
					.WithBody(
						Block(
							SingletonList<StatementSyntax>(
								ExpressionStatement(
									AssignmentExpression(
										SyntaxKind.SimpleAssignmentExpression,
										IdentifierName(fieldName),
										IdentifierName(constructorArgName))))));

			editor.InsertMembers(declaration, 0, new SyntaxNode[] { fieldDeclaration, constructor });

			var newdoc = await editor.GetChangedDocument().GetTextAsync();
			return editor.GetChangedDocument();
		}

		public static async Task<Solution> ChangeMemberAccessibility(Solution solution, ISymbol memberSymbol, Accessibility accessibility, CancellationToken cancellationToken)
		{
			var editor = SymbolEditor.Create(solution);
			await editor.EditAllDeclarationsAsync(memberSymbol, (docEditor, syntaxNode) =>
			{
				docEditor.SetAccessibility(syntaxNode, accessibility);
			}, cancellationToken).ConfigureAwait(false);
			return editor.ChangedSolution;
		}

		public static async Task<Solution> ChangeMemberStaticModifier(Solution solution, ISymbol memberSymbol, bool isStatic, CancellationToken cancellationToken)
		{
			var editor = SymbolEditor.Create(solution);
			await editor.EditAllDeclarationsAsync(memberSymbol, (docEditor, syntaxNode) =>
			{
				var newMods = DeclarationModifiers.From(memberSymbol).WithIsStatic(isStatic);
				if (memberSymbol is IPropertySymbol propertySymbol && propertySymbol.IsReadOnly)
				{
					// Looks like there's a bug in Roslyn where SetModifiers applies the 'readonly'
					// keyword to a get-only property, producing illegal syntax.
					newMods = newMods.WithIsReadOnly(false);
				}
				docEditor.SetModifiers(syntaxNode, newMods);
			}, cancellationToken).ConfigureAwait(false);

			return editor.ChangedSolution;
		}

		public static async Task<Solution> ChangeMemberType(Solution solution, ISymbol memberSymbol, ITypeSymbol type, CancellationToken cancellationToken)
		{
			var editor = SymbolEditor.Create(solution);
			await editor.EditAllDeclarationsAsync(memberSymbol, (docEditor, syntaxNode) =>
			{
				docEditor.SetType(syntaxNode, docEditor.Generator.TypeExpression(type));
			}, cancellationToken).ConfigureAwait(false);

			return editor.ChangedSolution;
		}

		public static async Task<Document> RemoveNodeAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			editor.RemoveNode(node);

			return editor.GetChangedDocument();
		}

		public static async Task<Document> SetBaseClass(Document document, ClassDeclarationSyntax declaration, string baseType, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var generator = editor.Generator;
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
			var baseTypeNode = generator.TypeExpression(semanticModel.Compilation.GetTypeByMetadataName(baseType));
			var baseTypes = generator.GetBaseAndInterfaceTypes(declaration);

			SyntaxNode updatedDeclaration;
			if (baseTypes?.Count == 0 || semanticModel.GetTypeInfo(baseTypes[0], cancellationToken).Type?.TypeKind != TypeKind.Class)
				updatedDeclaration = generator.AddBaseType(declaration, baseTypeNode);
			else
				updatedDeclaration = generator.ReplaceNode(declaration, baseTypes[0], baseTypeNode);

			editor.ReplaceNode(declaration, updatedDeclaration);
			return editor.GetChangedDocument();
		}
	}
}
