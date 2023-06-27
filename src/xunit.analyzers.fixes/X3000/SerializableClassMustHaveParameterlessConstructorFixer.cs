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

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class SerializableClassMustHaveParameterlessConstructorFixer : BatchedCodeFixProvider
{
	public const string Key_GenerateOrUpdateConstructor = "xUnit3001_GenerateOrUpdateConstructor";

	static readonly LiteralExpressionSyntax obsoleteText;

	static SerializableClassMustHaveParameterlessConstructorFixer() =>
		obsoleteText = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"));

	public SerializableClassMustHaveParameterlessConstructorFixer() :
		base(Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classDeclaration is null)
			return;

		var parameterlessCtor = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);

		context.RegisterCodeFix(
			CodeAction.Create(
				parameterlessCtor is null ? "Create public constructor" : "Make parameterless constructor public",
				ct => CreateOrUpdateConstructor(context.Document, classDeclaration, ct),
				Key_GenerateOrUpdateConstructor
			),
			context.Diagnostics
		);
	}

	async Task<Document> CreateOrUpdateConstructor(
		Document document,
		ClassDeclarationSyntax declaration,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var generator = editor.Generator;
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
		var parameterlessCtor = declaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);

		if (parameterlessCtor is null)
		{
			var obsoleteAttribute = generator.Attribute(Constants.Types.System.ObsoleteAttribute, obsoleteText);
			var newCtor = generator.ConstructorDeclaration();
			newCtor = generator.WithAccessibility(newCtor, Accessibility.Public);
			newCtor = generator.AddAttributes(newCtor, obsoleteAttribute);
			editor.InsertMembers(declaration, 0, new[] { newCtor });
		}
		else
		{
			var updatedCtor = generator.WithAccessibility(parameterlessCtor, Accessibility.Public);

			var hasObsolete =
				parameterlessCtor
					.AttributeLists
					.SelectMany(al => al.Attributes)
					.Any(@as => semanticModel.GetTypeInfo(@as, cancellationToken).Type?.ToDisplayString() == Constants.Types.System.ObsoleteAttribute);

			if (!hasObsolete)
			{
				var obsoleteAttribute = generator.Attribute(Constants.Types.System.ObsoleteAttribute, obsoleteText);
				updatedCtor = generator.AddAttributes(updatedCtor, obsoleteAttribute);
			}

			editor.ReplaceNode(parameterlessCtor, updatedCtor);
		}

		return editor.GetChangedDocument();
	}
}
