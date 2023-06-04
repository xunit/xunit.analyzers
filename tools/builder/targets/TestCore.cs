using System.IO;
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
		File.Delete(Path.Combine(resultPath, "netcore.trx"));

		return context.Exec("dotnet", $"test src/xunit.analyzers.tests --framework net6.0 --configuration {context.ConfigurationText} --no-build --logger trx;LogFileName=netcore.trx --results-directory \"{resultPath}\" --verbosity {context.Verbosity}");
	}
}
