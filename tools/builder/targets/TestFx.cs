using System;
using System.IO;
using System.Threading.Tasks;
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

		if (context.NeedMono)
		{
			context.WriteLineColor(ConsoleColor.Yellow, "Skipping .NET Framework tests on non-Windows OSes.");
			Console.WriteLine();

			return Task.CompletedTask;
		}

		var resultPath = Path.Combine(context.BaseFolder, "artifacts", "test");
		File.Delete(Path.Combine(resultPath, "netfx.trx"));

		return context.Exec("dotnet", $"test src/xunit.analyzers.tests --framework net472 --configuration {context.ConfigurationText} --no-build --logger trx;LogFileName=netfx.trx --results-directory \"{resultPath}\" --verbosity {context.Verbosity}");
	}
}
