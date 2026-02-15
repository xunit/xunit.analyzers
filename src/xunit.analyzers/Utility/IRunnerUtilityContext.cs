using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Context for types that that originated in <c>xunit.runner.utility</c> in v2,
/// and moved in v3 to one of <c>xunit.v3.runner.utility</c> or <c>xunit.v3.runner.utility.aot</c>.
/// </summary>
public interface IRunnerUtilityContext
{
	/// <summary>
	/// Gets a reference to type <c>Xunit.Sdk.LongLivedMarshalByRefObject</c>, if available.
	/// </summary>
	INamedTypeSymbol? LongLivedMarshalByRefObjectType { get; }

	/// <summary>
	/// Gets a description of the target platform for the runner utility (i.e., "net452"). This is
	/// typically extracted from the assembly name (i.e., "xunit.runner.utility.net452").
	/// </summary>
	string Platform { get; }

	/// <summary>
	/// Gets the version number of the runner utility assembly.
	/// </summary>
	Version Version { get; }
}
