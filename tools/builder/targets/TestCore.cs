using System.IO;
using System.Threading.Tasks;

[Target(BuildTarget.TestCore,
        BuildTarget.Build)]
public static class TestCore
{
    public static Task OnExecute(BuildContext context)
    {
        context.BuildStep("Running .NET Core tests");

        var resultPath = Path.Combine(context.BaseFolder, "artifacts", "test");
        File.Delete(Path.Combine(resultPath, "netcore.trx"));

        return context.Exec("dotnet", $"test test/xunit.analyzers.tests --framework netcoreapp2.1 --configuration {context.ConfigurationText} --no-build --logger trx;LogFileName=netcore.trx --results-directory \"{resultPath}\"");
    }
}
