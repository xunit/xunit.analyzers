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
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class SerializableClassMustHaveParameterlessConstructorFixer : XunitCodeFixProvider
{
	public const string Key_GenerateOrUpdateConstructor = "xUnit3001_GenerateOrUpdateConstructor";

	static readonly LiteralExpressionSyntax obsoleteText =
		LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"));

	public SerializableClassMustHaveParameterlessConstructorFixer() :
		base(Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classDeclaration is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		if (!diagnostic.Properties.TryGetValue(Constants.Properties.IsCtorObsolete, out var isCtorObsolete))
			return;

		var parameterlessCtor = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);

		context.RegisterCodeFix(
			CodeAction.Create(
				parameterlessCtor is null ? "Create public constructor" : "Make parameterless constructor public",
				ct => CreateOrUpdateConstructor(context.Document, classDeclaration, isCtorObsolete == bool.TrueString, ct),
				Key_GenerateOrUpdateConstructor
			),
			context.Diagnostics
		);
	}

	static async Task<Document> CreateOrUpdateConstructor(
		Document document,
		ClassDeclarationSyntax declaration,
		bool isCtorObsolete,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var generator = editor.Generator;
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var parameterlessCtor = declaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);

		if (parameterlessCtor is null)
		{
			var newCtor = generator.ConstructorDeclaration();
			newCtor = generator.WithAccessibility(newCtor, Accessibility.Public);

			if (isCtorObsolete)
			{
				var obsoleteAttribute = generator.Attribute(Constants.Types.System.ObsoleteAttribute, obsoleteText);
				newCtor = generator.AddAttributes(newCtor, obsoleteAttribute);
			}

			editor.InsertMembers(declaration, 0, [newCtor]);
		}
		else
		{
			var updatedCtor = generator.WithAccessibility(parameterlessCtor, Accessibility.Public);

			if (isCtorObsolete)
			{
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
			}

			editor.ReplaceNode(parameterlessCtor, updatedCtor);
		}

		return editor.GetChangedDocument();
	}
}
