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

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class SerializableClassMustHaveParameterlessConstructorFixer : CodeFixProvider
    {
        const string ObsoleteMessage = "Called by the de-serializer; should only be called by deriving classes for de-serialization purposes";
        const string Title = "Create/update constructor";

        static readonly LiteralExpressionSyntax obsoleteText;

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        static SerializableClassMustHaveParameterlessConstructorFixer()
        {
            obsoleteText = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(ObsoleteMessage));
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: ct => RunFix(context.Document, classDeclaration, ct),
                    equivalenceKey: Title),
                context.Diagnostics);
        }

        async Task<Document> RunFix(Document document, ClassDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var generator = editor.Generator;
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var parameterlessCtor = declaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);
            var obsoleteAttribute = generator.Attribute(Constants.Types.SystemObsoleteAttribute, obsoleteText);

            if (parameterlessCtor == null)
            {
                var constructor = generator.ConstructorDeclaration(accessibility: Accessibility.Public);
                var constructorWithAttributes = generator.AddAttributes(constructor, obsoleteAttribute);
                var updatedDeclaration = editor.Generator.InsertMembers(declaration, 0, constructorWithAttributes);
                editor.ReplaceNode(declaration, updatedDeclaration);
            }
            else
            {
                var updatedCtor = generator.WithAccessibility(parameterlessCtor, Accessibility.Public);

                var hasObsolete = parameterlessCtor.AttributeLists
                                                   .SelectMany(al => al.Attributes)
                                                   .Any(@as => semanticModel.GetTypeInfo(@as, cancellationToken).Type?.ToDisplayString() == Constants.Types.SystemObsoleteAttribute);
                if (!hasObsolete)
                    updatedCtor = generator.AddAttributes(updatedCtor, obsoleteAttribute);

                editor.ReplaceNode(parameterlessCtor, updatedCtor);
            }

            return editor.GetChangedDocument();
        }
    }
}
