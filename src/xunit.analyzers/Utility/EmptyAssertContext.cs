using System;

namespace Xunit.Analyzers;

public class EmptyAssertContext : IAssertContext
{
	EmptyAssertContext()
	{ }

	public static EmptyAssertContext Instance { get; } = new();

	public bool SupportsAssertFail => false;

	public Version Version { get; } = new();
}
