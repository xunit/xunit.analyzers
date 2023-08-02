using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class LocalFunctionsCannotBeTestFunctionsFixer : BatchedCodeFixProvider
{
	public const string Key_RemoveAttribute = "xUnit1029_RemoveAttribute";

	public LocalFunctionsCannotBeTestFunctionsFixer() :
		base(Descriptors.X1029_LocalFunctionsCannotBeTestFunctions.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var localFunctionDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<LocalFunctionStatementSyntax>();
		if (localFunctionDeclaration is null)
			return;

		context.RegisterCodeFix(
			new RemoveAttributesOfTypeCodeAction(
				"Remove attribute",
				Key_RemoveAttribute,
				context.Document,
				localFunctionDeclaration.AttributeLists,
				Constants.Types.Xunit.FactAttribute
			),
			context.Diagnostics
		);
	}
}
