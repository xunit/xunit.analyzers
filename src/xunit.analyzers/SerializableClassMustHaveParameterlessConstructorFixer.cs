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

        static readonly AttributeListSyntax attributeList;
        static readonly SyntaxList<AttributeListSyntax> attributes;
        static readonly BlockSyntax body;
        static readonly SyntaxTokenList modifiers;
        static readonly ParameterListSyntax parameterList;

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Descriptors.X3001_SerializableClassMustHaveParameterlessConstructor.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        static SerializableClassMustHaveParameterlessConstructorFixer()
        {
            var attributeName = SyntaxFactory.ParseName(Constants.Types.SystemObsolete);
            var obsoleteText = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(ObsoleteMessage));
            var attributeArg = SyntaxFactory.AttributeArgument(obsoleteText);
            var attributeArgs = SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(attributeArg));
            var attribute = SyntaxFactory.Attribute(attributeName, attributeArgs);
            var attributeSyntaxList = SyntaxFactory.SingletonSeparatedList(attribute);
            attributeList = SyntaxFactory.AttributeList(attributeSyntaxList);
            attributes = SyntaxFactory.SingletonList(attributeList);
            modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            parameterList = SyntaxFactory.ParameterList();
            body = SyntaxFactory.Block();
        }

        static ConstructorDeclarationSyntax EmptyPublicConstructor(SyntaxToken identifier)
            => SyntaxFactory.ConstructorDeclaration(attributes, modifiers, identifier, parameterList, null, body);

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
            var parameterlessCtor = declaration.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault(c => c.ParameterList.Parameters.Count == 0);

            if (parameterlessCtor == null)
            {
                var updatedDeclaration = declaration.AddMembers(EmptyPublicConstructor(declaration.Identifier));
                editor.ReplaceNode(declaration, updatedDeclaration);
            }
            else
            {
                var updatedCtor = parameterlessCtor.WithModifiers(modifiers);

                var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                var hasObsolete = parameterlessCtor.AttributeLists
                                                   .SelectMany(al => al.Attributes)
                                                   .Any(@as => semanticModel.GetTypeInfo(@as, cancellationToken).Type?.ToDisplayString() == Constants.Types.SystemObsoleteAttribute);
                if (!hasObsolete)
                    updatedCtor = updatedCtor.AddAttributeLists(attributeList);

                editor.ReplaceNode(parameterlessCtor, updatedCtor);
            }

            return editor.GetChangedDocument();
        }
    }
}
