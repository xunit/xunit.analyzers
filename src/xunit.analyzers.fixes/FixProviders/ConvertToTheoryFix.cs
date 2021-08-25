using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers.FixProviders
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class ConvertToTheoryFix : CodeFixProvider
	{
		const string title = "Convert to Theory";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.X1001_FactMethodMustNotHaveParameters.Id,
				Descriptors.X1005_FactMethodShouldNotHaveTestData.Id
			);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

			context.RegisterCodeFix(
				new ConvertAttributeCodeAction(
					title,
					context.Document,
					methodDeclaration.AttributeLists,
					fromTypeName: Constants.Types.XunitTheoryAttribute,
					toTypeName: Constants.Types.XunitFactAttribute
				),
				context.Diagnostics
			);
		}
	}
}
