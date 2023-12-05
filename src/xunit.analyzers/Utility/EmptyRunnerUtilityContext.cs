using System;

namespace Xunit.Analyzers;

public class EmptyRunnerUtilityContext : IRunnerUtilityContext
{
	EmptyRunnerUtilityContext()
	{ }

	public static EmptyRunnerUtilityContext Instance { get; } = new();

	public string Platform => "N/A";

	public Version Version { get; } = new();
}
