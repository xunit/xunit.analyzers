using System;
using System.IO;
using System.Threading.Tasks;
using SimpleExec;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestFx,
	BuildTarget.Build
)]
public static class TestFx
{
	public static Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running .NET Framework tests");

		var consoleRunner = Path.Combine(context.NuGetPackageCachePath, "xunit.v3.runner.console", "0.1.1-pre.322", "tools", "net472", "xunit.v3.runner.console.exe");

		if (!File.Exists(consoleRunner))
		{
			context.WriteLineColor(ConsoleColor.Red, $"Cannot run .NET Framework tests because path '{consoleRunner}' does not exist");
			throw new ExitCodeException(-1);
		}

		var resultPath = Path.Combine(context.BaseFolder, "artifacts", "test");
		var trxFilePath = Path.Combine(resultPath, "netfx.trx");
		File.Delete(trxFilePath);

		return context.Exec(
			consoleRunner,
			$"xunit.analyzers.tests.dll -trx \"{trxFilePath}\"",
			workingDirectory: $"src/xunit.analyzers.tests/bin/{context.ConfigurationText}/net472"
		);
	}
}
