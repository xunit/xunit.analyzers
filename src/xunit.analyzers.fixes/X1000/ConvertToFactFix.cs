using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class ConvertToFactFix : XunitCodeFixProvider
{
	public const string Key_ConvertToFact = "xUnit1003_xUnit1006_ConvertToFact";

	public ConvertToFactFix() :
		base(
			Descriptors.X1003_TheoryMethodMustHaveTestData.Id,
			Descriptors.X1006_TheoryMethodShouldHaveParameters.Id
		)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (methodDeclaration is null)
			return;

		context.RegisterCodeFix(
			new ConvertAttributeCodeAction(
				"Convert to [Fact]",
				Key_ConvertToFact,
				context.Document,
				methodDeclaration.AttributeLists,
				fromTypeName: Constants.Types.Xunit.TheoryAttribute,
				toTypeName: Constants.Types.Xunit.FactAttribute
			),
			context.Diagnostics
		);
	}
}
