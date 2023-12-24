using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools.Targets;

[Target(
	BuildTarget.TestCore,
	BuildTarget.Build
)]
public static class TestCore
{
	public static Task OnExecute(BuildContext context)
	{
		context.BuildStep("Running .NET Core tests");

		var resultPath = Path.Combine(context.BaseFolder, "artifacts", "test");

		var testDLLs =
			Directory
				.GetFiles(Path.Join(context.BaseFolder, "src"), "*.tests*.csproj", SearchOption.AllDirectories)
				.Select(csproj => '"' + Path.Combine(Path.GetDirectoryName(csproj)!, "bin", context.ConfigurationText, "net8.0", Path.GetFileNameWithoutExtension(csproj) + ".dll") + '"');

		File.Delete(Path.Combine(resultPath, "netcore.trx"));

		return context.Exec("dotnet", $"vstest {string.Join(" ", testDLLs)} --logger:\"trx;LogFileName=netcore.trx\" --ResultsDirectory:\"{resultPath}\" --Parallel");
	}
}
