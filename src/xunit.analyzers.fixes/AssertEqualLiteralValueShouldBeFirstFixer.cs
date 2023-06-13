using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertEqualLiteralValueShouldBeFirstFixer : CodeFixProvider
	{
		const string title = "Swap Arguments";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root is null)
				return;

			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			if (invocation is null)
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => SwapArguments(context.Document, invocation, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		async Task<Document> SwapArguments(
			Document document,
			InvocationExpressionSyntax invocation,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			if (invocation is not null)
			{
				var arguments = invocation.ArgumentList.Arguments;

				if (arguments.Count >= 2)
				{
					ArgumentSyntax expectedArg, actualArg;
					if (arguments.All(x => x.NameColon is not null))
					{
						expectedArg = arguments.Single(x => x.NameColon?.Name.Identifier.ValueText == Constants.AssertArguments.Expected);
						actualArg = arguments.Single(x => x.NameColon?.Name.Identifier.ValueText == Constants.AssertArguments.Actual);
					}
					else
					{
						expectedArg = arguments[0];
						actualArg = arguments[1];
					}

					editor.ReplaceNode(expectedArg, expectedArg.WithExpression(actualArg.Expression));
					editor.ReplaceNode(actualArg, actualArg.WithExpression(expectedArg.Expression));
				}
			}

			return editor.GetChangedDocument();
		}
	}
}
