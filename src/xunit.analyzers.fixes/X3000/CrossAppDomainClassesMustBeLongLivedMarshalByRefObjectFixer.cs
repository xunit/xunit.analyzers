using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer : BatchedCodeFixProvider
{
	public const string Key_SetBaseType = "xUnit3000_SetBaseType";

	public CrossAppDomainClassesMustBeLongLivedMarshalByRefObjectFixer() :
		base(Descriptors.X3000_CrossAppDomainClassesMustBeLongLivedMarshalByRefObject.Id)
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
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.NewBaseType, out var newBaseType) || newBaseType is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Set Base Type",
				ct => context.Document.SetBaseClass(classDeclaration, newBaseType, ct),
				Key_SetBaseType
			),
			context.Diagnostics
		);
	}
}
