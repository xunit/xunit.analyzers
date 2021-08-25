using System.Collections.Immutable;
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

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class SerializableClassMustHaveParameterlessConstructorFixer : CodeFixProvider
	{
		const string title = "Create/update constructor";

		static readonly LiteralExpressionSyntax obsoleteText;

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		static SerializableClassMustHaveParameterlessConstructorFixer()
		{
			obsoleteText = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes"));
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();

			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedDocument: ct => AddObsoleteAttribute(context.Document, classDeclaration, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		async Task<Document> AddObsoleteAttribute(
			Document document,
			ClassDeclarationSyntax declaration,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var generator = editor.Generator;
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
			var parameterlessCtor = declaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);
			var updatedCtor = generator.WithAccessibility(parameterlessCtor, Accessibility.Public);

			var hasObsolete =
				parameterlessCtor
					.AttributeLists
					.SelectMany(al => al.Attributes)
					.Any(@as => semanticModel.GetTypeInfo(@as, cancellationToken).Type?.ToDisplayString() == Constants.Types.SystemObsoleteAttribute);

			if (!hasObsolete)
			{
				var obsoleteAttribute = generator.Attribute(Constants.Types.SystemObsoleteAttribute, obsoleteText);
				updatedCtor = generator.AddAttributes(updatedCtor, obsoleteAttribute);
			}

			editor.ReplaceNode(parameterlessCtor, updatedCtor);

			return editor.GetChangedDocument();
		}
	}
}
