using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Xunit.Analyzers.Fixes;

public abstract class XunitCodeFixProvider(params string[] diagnostics) :
	CodeFixProvider
{
#pragma warning disable IDE0305  // Cannot convert this due to Roslyn 3.11 vs. 4.11 dependencies
	public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = diagnostics.ToImmutableArray();
#pragma warning restore IDE0305

	public sealed override FixAllProvider? GetFixAllProvider() =>
		null;
}
