using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ClassDataAttributeMustPointAtValidClassFixer : BatchedCodeFixProvider
{
	public const string Key_FixDataClass = "xUnit1007_FixDataClass";

	public ClassDataAttributeMustPointAtValidClassFixer() :
		base(Descriptors.X1007_ClassDataAttributeMustPointAtValidClass.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		var typeOfExpression = root.FindNode(context.Span).FirstAncestorOrSelf<TypeOfExpressionSyntax>();
		if (typeOfExpression is null)
			return;

		if (semanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type is INamedTypeSymbol typeSymbol)
			if (typeSymbol.TypeKind == TypeKind.Class && typeSymbol.Locations.Any(l => l.IsInSource))
				context.RegisterCodeFix(
					CodeAction.Create(
						"Fix data class",
						ct => FixClass(context.Document.Project.Solution, typeSymbol, ct),
						Key_FixDataClass
					),
					context.Diagnostics
				);
	}

	static async Task<Solution> FixClass(
		Solution solution,
		INamedTypeSymbol typeSymbol,
		CancellationToken cancellationToken)
	{
		var symbolEditor = SymbolEditor.Create(solution);

		await symbolEditor.EditOneDeclarationAsync(typeSymbol, async (editor, declaration, ct) =>
		{
			var classDeclaration = (ClassDeclarationSyntax)declaration;
			var compilation = editor.SemanticModel.Compilation;
			var generator = editor.Generator;

			if (typeSymbol.IsAbstract)
				editor.SetModifiers(declaration, DeclarationModifiers.From(typeSymbol).WithIsAbstract(false));

			var ctor = typeSymbol.InstanceConstructors.FirstOrDefault(c => c.Parameters.Length == 0);
			if (ctor is null)
				editor.AddMember(classDeclaration, generator.ConstructorDeclaration(accessibility: Accessibility.Public));
			else if (ctor.DeclaredAccessibility != Accessibility.Public && !(ctor.IsImplicitlyDeclared && typeSymbol.IsAbstract))
			{
				// Make constructor public unless it's implicit and the class was abstract. Making the class non-abstract will make the implicit constructor public
				var ctorSyntaxRef = ctor.DeclaringSyntaxReferences.FirstOrDefault();
				if (ctorSyntaxRef is not null)
					editor.SetAccessibility(await ctorSyntaxRef.GetSyntaxAsync(ct).ConfigureAwait(false), Accessibility.Public);
			}

			var iEnumerableOfObjectArray = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
			if (!iEnumerableOfObjectArray.IsAssignableFrom(typeSymbol))
				editor.AddInterfaceType(classDeclaration, generator.TypeExpression(iEnumerableOfObjectArray));
		}, cancellationToken).ConfigureAwait(false);

		return symbolEditor.ChangedSolution;
	}
}
