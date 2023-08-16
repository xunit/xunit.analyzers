using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class TestClassCannotBeNestedInGenericClassFixer : BatchedCodeFixProvider
{
	public const string Key_ExtractTestClass = "xUnit1032_TestClassCannotBeNestedInGenericClass";

	public TestClassCannotBeNestedInGenericClassFixer() :
		base(Descriptors.X1032_TestClassCannotBeNestedInGenericClass.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classDeclaration is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Extract test class from parent class",
				ct => context.Document.ExtractNodeFromParent(classDeclaration, ct),
				Key_ExtractTestClass
			),
			context.Diagnostics
		);
	}
}
