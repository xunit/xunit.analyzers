using System;
using System.IO;
using System.Linq;
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
			context.WriteLineColor(ConsoleColor.Yellow, ".NET Framework tests are not supported on Mono");
			Console.WriteLine();

			return Task.CompletedTask;
		}

		var resultPath = Path.Combine(context.BaseFolder, "artifacts", "test");

		var testDLLs =
			Directory
				.GetFiles(Path.Join(context.BaseFolder, "src"), "*.tests*.csproj", SearchOption.AllDirectories)
				.Select(csproj => '"' + Path.Combine(Path.GetDirectoryName(csproj)!, "bin", context.ConfigurationText, "net472", Path.GetFileNameWithoutExtension(csproj) + ".dll") + '"');

		File.Delete(Path.Combine(resultPath, "netfx.trx"));

		return context.Exec("dotnet", $"vstest {string.Join(" ", testDLLs)} --logger:\"trx;LogFileName=netfx.trx\" --ResultsDirectory:\"{resultPath}\" --Parallel");
	}
}
