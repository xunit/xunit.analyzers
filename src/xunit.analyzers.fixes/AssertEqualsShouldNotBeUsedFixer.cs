using System;
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
	public class AssertEqualsShouldNotBeUsedFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
			= ImmutableArray.Create(Descriptors.X2001_AssertEqualsShouldNotBeUsed.Id);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			string replacement = null;
			switch (context.Diagnostics.First().Properties[AssertEqualsShouldNotBeUsed.MethodName])
			{
				case AssertEqualsShouldNotBeUsed.EqualsMethod:
					replacement = "Equal";
					break;

				case AssertEqualsShouldNotBeUsed.ReferenceEqualsMethod:
					replacement = "Same";
					break;
			}

			if (replacement != null && invocation.Expression is MemberAccessExpressionSyntax)
			{
				var title = string.Format(titleTemplate, replacement);
				context.RegisterCodeFix(
					new UseDifferentMethodCodeAction(title, context.Document, invocation, replacement),
					context.Diagnostics);
			}
		}
	}
}
