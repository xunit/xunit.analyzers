using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Context for types that live in one of <c>xunit.v3.runner.common</c> or <c>xunit.v3.runner.common.aot</c>.
/// </summary>
public interface IRunnerCommonContextV3
{
	/// <summary>
	/// Gets a reference to type <c>IRunnerReport</c>, if available.
	/// </summary>
	INamedTypeSymbol? IRunnerReporterType { get; }

	/// <summary>
	/// Gets the version number of the runner common assembly.
	/// </summary>
	Version Version { get; }
}
