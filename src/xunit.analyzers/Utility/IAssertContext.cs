using System;

namespace Xunit.Analyzers;

public interface IAssertContext
{
	/// <summary>
	/// Gets a flag indicating whether <c>Assert.Fail</c> is supported.
	/// </summary>
	bool SupportsAssertFail { get; }

	/// <summary>
	/// Gets the version number of the assertion assembly.
	/// </summary>
	Version Version { get; }
}
