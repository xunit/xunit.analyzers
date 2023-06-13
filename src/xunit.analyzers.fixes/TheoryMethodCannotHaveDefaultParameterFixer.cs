using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class TheoryMethodCannotHaveDefaultParameterFixer : CodeFixProvider
	{
		const string titleTemplate = "Remove Parameter '{0}' Default";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X1023_TheoryMethodCannotHaveDefaultParameter.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root is null)
				return;

			var parameter = root.FindNode(context.Span).FirstAncestorOrSelf<ParameterSyntax>();
			if (parameter is null || parameter.Default is null)
				return;

			var parameterName = parameter.Identifier.Text;
			var title = string.Format(titleTemplate, parameterName);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					ct => context.Document.RemoveNode(parameter.Default, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}
	}
}
