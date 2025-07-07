using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class RemoveMethodParameterFix : XunitCodeFixProvider
{
	public const string Key_RemoveParameter = "xUnit1022_xUnit1026_RemoveParameter";

	public RemoveMethodParameterFix() :
		base(
			Descriptors.X1022_TheoryMethodCannotHaveParameterArray.Id,
			Descriptors.X1026_TheoryMethodShouldUseAllParameters.Id
		)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var parameter = root.FindNode(context.Span).FirstAncestorOrSelf<ParameterSyntax>();
		if (parameter is null)
			return;

		var parameterName = parameter.Identifier.Text;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => context.Document.RemoveNode(parameter, ct),
				Key_RemoveParameter,
				"Remove parameter '{0}'", parameterName
			),
			context.Diagnostics
		);
	}
}
