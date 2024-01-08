using System.Composition;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class TheoryMethodCannotHaveDefaultParameterFixer : BatchedCodeFixProvider
{
	public const string Key_RemoveParameterDefault = "xUnit1023_RemoveParameterDefault";

	public TheoryMethodCannotHaveDefaultParameterFixer() :
		base(Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var parameter = root.FindNode(context.Span).FirstAncestorOrSelf<ParameterSyntax>();
		if (parameter is null || parameter.Default is null)
			return;

		var parameterName = parameter.Identifier.Text;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format(CultureInfo.CurrentCulture, "Remove parameter '{0}' default", parameterName),
				ct => context.Document.RemoveNode(parameter.Default, ct),
				Key_RemoveParameterDefault
			),
			context.Diagnostics
		);
	}
}
