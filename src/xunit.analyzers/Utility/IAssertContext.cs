using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

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
