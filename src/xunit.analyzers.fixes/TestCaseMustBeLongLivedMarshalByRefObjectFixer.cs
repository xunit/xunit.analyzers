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
	public class TestCaseMustBeLongLivedMarshalByRefObjectFixer : CodeFixProvider
	{
		const string title = "Set Base Type";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X3000_TestCaseMustBeLongLivedMarshalByRefObject.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var classDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();

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
}
