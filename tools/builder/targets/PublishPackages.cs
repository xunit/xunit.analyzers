using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[Target(BuildTarget.PublishPackages,
        BuildTarget.DownloadNuGet, BuildTarget.Packages)]
public static class PublishPackages
{
    public static async Task OnExecute(BuildContext context)
    {
        context.BuildStep("Publishing NuGet packages");

        var publishToken = Environment.GetEnvironmentVariable("PublishToken");
        if (string.IsNullOrWhiteSpace(publishToken))
        {
            context.WriteLineColor(ConsoleColor.Yellow, $"Skipping package publishing because environment variable 'PublishToken' is not set.{Environment.NewLine}");
            return;
        }

        var randomName = Guid.NewGuid().ToString("n");
        var args = $"nuget source add -Name {randomName} -Source https://nuget.pkg.github.com/xunit/index.json -UserName xunit -Password {publishToken}";
        var redactedArgs = args.Replace(publishToken, "[redacted]");
        await context.Exec(context.NuGetExe, args, redactedArgs);

        var packageFiles = Directory.GetFiles(context.PackageOutputFolder, "*.nupkg", SearchOption.AllDirectories)
                                    .OrderBy(x => x)
                                    .Select(x => x.Substring(context.BaseFolder.Length + 1));

        foreach (var packageFile in packageFiles)
            await context.Exec(context.NuGetExe, $"push -source {randomName} {packageFile}");
    }
}
