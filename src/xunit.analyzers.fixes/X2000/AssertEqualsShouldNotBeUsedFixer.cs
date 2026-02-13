using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertEqualsShouldNotBeUsedFixer : XunitCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2001_UseAlternateAssert";

	public AssertEqualsShouldNotBeUsedFixer() :
		base(Descriptors.X2001_AssertEqualsShouldNotBeUsed.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		if (invocation.Expression is MemberAccessExpressionSyntax)
			context.RegisterCodeFix(
				XunitCodeAction.UseDifferentAssertMethod(
					Key_UseAlternateAssert,
					context.Document,
					invocation,
					replacement
				),
				context.Diagnostics
			);
	}
}
