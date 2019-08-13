using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ClassDataAttributeMustPointAtValidClassFixer : CodeFixProvider
    {
        const string title = "Fix Data Class";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.X1007_ClassDataAttributeMustPointAtValidClass.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var typeOfExpression = root.FindNode(context.Span).FirstAncestorOrSelf<TypeOfExpressionSyntax>();
            var typeSymbol = (INamedTypeSymbol)semanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type;
            if (typeSymbol.TypeKind == TypeKind.Class && typeSymbol.Locations.Any(l => l.IsInSource))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        createChangedSolution: ct => FixClass(context.Document.Project.Solution, typeSymbol, ct),
                        equivalenceKey: title),
                    context.Diagnostics);
            }
        }

        private async Task<Solution> FixClass(Solution solution, INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
        {
            var symbolEditor = SymbolEditor.Create(solution);
            await symbolEditor.EditOneDeclarationAsync(typeSymbol, async (editor, declaration, ct) =>
            {
                var classDeclaration = (ClassDeclarationSyntax)declaration;
                var compilation = editor.SemanticModel.Compilation;
                var generator = editor.Generator;

                if (typeSymbol.IsAbstract)
                {
                    editor.SetModifiers(declaration, DeclarationModifiers.From(typeSymbol).WithIsAbstract(false));
                }

                var ctor = typeSymbol.InstanceConstructors.FirstOrDefault(c => c.Parameters.Length == 0);
                if (ctor == null)
                {
                    editor.AddMember(classDeclaration, generator.ConstructorDeclaration(accessibility: Accessibility.Public));
                }
                else if (ctor.DeclaredAccessibility != Accessibility.Public)
                {
                    // Make constructor public unless it's implicit and the class was abstract. Making the class non-abstract will make the implicit constructor public
                    if (!(ctor.IsImplicitlyDeclared && typeSymbol.IsAbstract))
                    {
                        var ctorSyntaxRef = ctor.DeclaringSyntaxReferences.FirstOrDefault();
                        editor.SetAccessibility(await ctorSyntaxRef.GetSyntaxAsync(ct).ConfigureAwait(false), Accessibility.Public);
                    }
                }

                var iEnumerableOfObjectArray = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
                if (!iEnumerableOfObjectArray.IsAssignableFrom(typeSymbol))
                {
                    editor.AddInterfaceType(classDeclaration, generator.TypeExpression(iEnumerableOfObjectArray));
                }

            }, cancellationToken).ConfigureAwait(false);
            return symbolEditor.ChangedSolution;
        }
    }
}
