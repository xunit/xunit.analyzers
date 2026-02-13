using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class FactMethodShouldNotHaveTestDataFixer : XunitCodeFixProvider
{
	public const string Key_RemoveDataAttributes = "xUnit1005_RemoveDataAttributes";

	public FactMethodShouldNotHaveTestDataFixer() :
		base(Descriptors.X1005_FactMethodShouldNotHaveTestData.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (methodDeclaration is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		if (!diagnostic.Properties.TryGetValue(Constants.Properties.DataAttributeTypeName, out var dataAttributeTypeName) || dataAttributeTypeName is null)
			return;

		context.RegisterCodeFix(
			new RemoveAttributesOfTypeCodeAction(
				"Remove data attributes",
				Key_RemoveDataAttributes,
				context.Document,
				methodDeclaration.AttributeLists,
				dataAttributeTypeName
			),
			context.Diagnostics
		);
	}
}
