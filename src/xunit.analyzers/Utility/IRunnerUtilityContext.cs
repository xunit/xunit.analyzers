using System;

namespace Xunit.Analyzers;

public interface IRunnerUtilityContext
{
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
