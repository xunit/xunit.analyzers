using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Context for types that that originated in <c>xunit.assert</c> or <c>xunit.assert.source</c> in v2,
/// and moved in v3 to one of <c>xunit.v3.assert</c>, <c>xunit.v3.assert.aot</c>, or <c>xunit.v3.assert.source</c>.
/// </summary>
public interface IAssertContext
{
	/// <summary>
	/// Gets a reference to type <c>Assert</c>, if available.
	/// </summary>
	INamedTypeSymbol? AssertType { get; }

	/// <summary>
	/// Gets a flag indicating whether <c>Assert.Fail</c> is supported.
	/// </summary>
	bool SupportsAssertFail { get; }

	/// <summary>
	/// Gets a flag indicating whether <c>Assert.Null</c> and <c>Assert.NotNull</c> supports
	/// unsafe pointers.
	/// </summary>
	bool SupportsAssertNullWithPointers { get; }

	/// <summary>
	/// Gets a flag indicating whether <c>Assert.IsType</c> and <c>Assert.IsNotType</c>
	/// support inexact matches (soft-deprecating <c>Assert.IsAssignableFrom</c>
	/// and <c>Assert.IsNotAssignableFrom</c>).
	/// </summary>
	bool SupportsInexactTypeAssertions { get; }

	/// <summary>
	/// Gets the version number of the assertion assembly.
	/// </summary>
	Version Version { get; }
}
