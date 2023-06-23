using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.Fixes;

public abstract class BatchedCodeFixProvider : CodeFixProvider
{
	protected BatchedCodeFixProvider(params string[] diagnostics) =>
		FixableDiagnosticIds = diagnostics.ToImmutableArray();

	public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }

	public sealed override FixAllProvider GetFixAllProvider() =>
		WellKnownFixAllProviders.BatchFixer;
}
