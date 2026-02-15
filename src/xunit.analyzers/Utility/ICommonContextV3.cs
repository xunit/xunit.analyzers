using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Context for types that live in one of <c>xunit.v3.common</c>, <c>xunit.v3.core</c>, or <c>xunit.v3.runner.common</c>
/// (or one <c>xunit.v3.common.aot</c>, <c>xunit.v3.core.aot</c>, or <c>xunit.v3.runner.common.aot</c>).
/// </summary>
public interface ICommonContextV3 : ICommonContext
{
	/// <summary>
	/// Gets a reference to type <c>IXunitSerializer</c>, if available.
	/// </summary>
	INamedTypeSymbol? IXunitSerializerType { get; }
}
