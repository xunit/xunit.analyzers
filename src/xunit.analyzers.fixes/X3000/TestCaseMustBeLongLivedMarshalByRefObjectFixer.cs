using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class TestCaseMustBeLongLivedMarshalByRefObjectFixer : BatchedCodeFixProvider
{
	const string title = "Set Base Type";

	public TestCaseMustBeLongLivedMarshalByRefObjectFixer() :
		base(Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classDeclaration is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic == null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.CanFix, out var canFixText))
			return;

		bool.TryParse(canFixText, out var canFix);
		if (!canFix)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				title: title,
				createChangedDocument: ct => context.Document.SetBaseClass(classDeclaration, Constants.Types.XunitLongLivedMarshalByRefObject, ct),
				equivalenceKey: title
			),
			context.Diagnostics
		);
	}
}
