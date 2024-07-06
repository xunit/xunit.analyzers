using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class EmptyRunnerUtilityContext : IRunnerUtilityContext
{
	EmptyRunnerUtilityContext()
	{ }

	public static EmptyRunnerUtilityContext Instance { get; } = new();

	public INamedTypeSymbol? LongLivedMarshalByRefObjectType => null;

	public string Platform => "N/A";

	public Version Version { get; } = new();
}
