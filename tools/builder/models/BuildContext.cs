using System;
using System.Collections.Generic;
using System.IO;

namespace Xunit.BuildTools.Models;

public partial class BuildContext
{
	string? consoleRunner;

	public string ConsoleRunner =>
		consoleRunner ?? throw new InvalidOperationException($"Tried to retrieve unset {nameof(BuildContext)}.{nameof(ConsoleRunner)}");

	public partial IReadOnlyList<string> GetSkippedAnalysisFolders() =>
		Array.Empty<string>();

	partial void Initialize()
	{
		consoleRunner = Path.Combine(NuGetPackageCachePath, "xunit.v3.runner.console", "3.2.1-pre.5", "tools", "net472", "xunit.v3.runner.console.exe");
		if (!File.Exists(consoleRunner))
			throw new InvalidOperationException($"Cannot find console runner at '{consoleRunner}'");
	}
}
