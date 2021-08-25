using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertSameShouldNotBeCalledOnValueTypesFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2005_AssertSameShouldNotBeCalledOnValueTypes.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var methodName = context.Diagnostics.First().Properties[Constants.Properties.MethodName];
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var replacement = methodName switch
			{
				Constants.Asserts.Same => Constants.Asserts.Equal,
				Constants.Asserts.NotSame => Constants.Asserts.NotEqual,
				_ => null,
			};

			if (replacement != null && invocation.Expression is MemberAccessExpressionSyntax)
			{
				var title = string.Format(titleTemplate, replacement);
				context.RegisterCodeFix(
					new UseDifferentMethodCodeAction(title, context.Document, invocation, replacement),
					context.Diagnostics
				);
			}
		}
	}
}
